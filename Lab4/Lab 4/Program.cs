using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleTables;
using LFTC;

namespace Lab4
{
    internal class Program
    {
        private FirstSet GenerateFirst(Grammar g)
        {
            FirstSet firstSet = new FirstSet();
            foreach (var a in g.NonTerminals)
            {
                firstSet[a] = FirstOf(a, g, firstSet);
            }

            return firstSet;
        }

        private HashSet<string> FirstOf(string symbol, Grammar g, FirstSet firstSet)
        {
            HashSet<string> first = new HashSet<string>();
            foreach (var production in g.Productions.Where(production => production.Start == symbol))
            {
                if (production.Rules.Count == 0)
                {
                    first.Add(Constants.EPSILON);
                    continue;
                }

                foreach (var rule in production.Rules)
                {
                    if (rule.Count == 0)
                    {
                        first.Add(Constants.EPSILON);
                        continue;
                    }

                    var firstSymbol = rule.First();
                    if (g.NonTerminals.Any(nonterminal => nonterminal == firstSymbol))
                    {
                        var gotFirsts = FirstOf(firstSymbol, g, firstSet);
                        foreach (var f in gotFirsts)
                            first.Add(f);
                    }
                    else
                    {
                        first.Add(firstSymbol);
                    }
                }
            }

            return first;
        }

        private FollowSet GenerateFollow(Grammar g, FirstSet firstSet)
        {
            FollowSet followSet = new FollowSet();
            foreach (var a in g.NonTerminals)
            {
                //   if (firstSet[a].Contains(Constants.EPSILON))
                if (!followSet.ContainsKey(a))
                    followSet[a] = FollowOf(a, a, g, firstSet, followSet);
            }

            return followSet;
        }

        private HashSet<string> FollowOperation(string symbol, HashSet<string> follow, Grammar g, Rule rule,
            Production p, int indexNonTerminal, string initialSymbol, FirstSet firstSet, FollowSet followSet)
        {
            if (indexNonTerminal == rule.Count - 1)
            {
                if (p.Start == symbol)
                    return follow;
                if (initialSymbol != p.Start)
                {
                    var temp = FollowOf(p.Start, initialSymbol, g, firstSet, followSet);
                    foreach (var i in temp)
                    {
                        follow.Add(i);
                    }
                }
            }
            else
            {
                string nextSymbol = rule[indexNonTerminal + 1];
                if (g.Terminals.Contains(nextSymbol))
                    follow.Add(nextSymbol);
                else
                {
                    if (initialSymbol != nextSymbol)
                    {
                        HashSet<string> firsts = new HashSet<string>(firstSet[nextSymbol]);
                        if (firsts.Contains(Constants.EPSILON))
                        {
                            var temp = FollowOf(nextSymbol, initialSymbol, g, firstSet, followSet);
                            foreach (var i in temp)
                            {
                                follow.Add(i);
                            }

                            firsts.Remove(Constants.EPSILON);
                        }

                        foreach (var i in firsts)
                        {
                            follow.Add(i);
                        }
                    }
                }
            }

            return follow;
        }

        private Stack<List<string>> conflicts = new Stack<List<string>>();

        private HashSet<string> FollowOf(string symbol, string initialSymbol, Grammar g, FirstSet firstSet,
            FollowSet followSet)
        {
            HashSet<string> follow = new HashSet<string>();
            if (symbol == g.StartingSymbol)
                follow.Add("$");
            foreach (var production in g.Productions)
            {
                foreach (var rule in production.Rules)
                {
                    List<string> ruleConflict = new List<string>
                    {
                        symbol
                    };
                    foreach (var s in rule)
                    {
                        ruleConflict.Add(s);
                    }

                    if (rule.Any(r => r == symbol) && !conflicts.Contains(ruleConflict))
                    {
                        conflicts.Push(ruleConflict);
                        int indexNonTerminal = rule.IndexOf(symbol);
                        var temp = FollowOperation(symbol, follow, g, rule, production, indexNonTerminal, initialSymbol,
                            firstSet, followSet);
                        foreach (var i in temp)
                            follow.Add(i);
                        List<string> sublist = rule.SubList(indexNonTerminal + 1, rule.Count);
                        if (sublist.Contains(symbol))
                        {
                            temp = FollowOperation(symbol, follow, g, rule, production,
                                indexNonTerminal + 1 + sublist.IndexOf(symbol), initialSymbol, firstSet, followSet);
                        }

                        conflicts.Pop();
                    }
                }
            }

            return follow;
        }

        public ParsingTable GetParsingTable(FirstSet first, FollowSet follow, Grammar grammar)
        {
            var parsingTable = new ParsingTable();
            parsingTable[Constants.END, Constants.END] = new Rule(new string[] { Constants.ACC }, -1);
            foreach (var terminal in grammar.Terminals)
                parsingTable[terminal, terminal] = new Rule(new string[] { Constants.POP }, -1);
            List<string> columnSymbols = new List<string>(grammar.Terminals)
            {
                Constants.END
            };
            foreach (var production in grammar.Productions)
            {
                var rowSymbol = production.Start;
                List<Rule> rules = production.Rules;
                foreach (var rule in rules)
                {
                    foreach (var columnSymbol in columnSymbols)
                    {
                        if (rule[0] == columnSymbol && columnSymbol != Constants.EPSILON)
                            parsingTable[rowSymbol, columnSymbol] = rule;
                        else if (grammar.NonTerminals.Contains(rule[0]) && first[rule[0]].Contains(columnSymbol))
                        {
                            if (!parsingTable.ContainsKeyPair(rowSymbol, columnSymbol))
                            {
                                parsingTable[rowSymbol, columnSymbol] = rule;
                            }
                        }
                        else if (rule[0] == Constants.EPSILON)
                        {
                            foreach (var b in follow[rowSymbol])
                            {
                                parsingTable[rowSymbol, b] = rule;
                            }
                        }
                        else
                        {
                            HashSet<string> firstOfRule = new HashSet<string>();
                            foreach (var symbol in rule)
                                if (grammar.NonTerminals.Contains(columnSymbol))
                                    foreach (var firstSymbol in first[columnSymbol])
                                        firstOfRule.Add(firstSymbol);
                            if (firstOfRule.Contains(Constants.EPSILON))
                            {
                                foreach (var _b in first[rowSymbol])
                                {
                                    string b = _b;
                                    if (b == Constants.EPSILON)
                                    {
                                        b = Constants.END;
                                    }

                                    if (!parsingTable.ContainsKeyPair(rowSymbol, b))
                                    {
                                        parsingTable[rowSymbol, b] = rule;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return parsingTable;
        }

        private static void Main(string[] args)
        {
            var g = Grammar.FromFile("grammar.txt");
            var p = new Program();
            var first = p.GenerateFirst(g);
            Console.WriteLine("FIRST");
            foreach (var i in first.Keys)
            {
                Console.Write($"{i}  ");
                Console.Write(string.Join(" ", first[i].ToArray()));
                Console.WriteLine();
            }

            Console.WriteLine("FOLLOW");
            var follow = p.GenerateFollow(g, first);
            foreach (var i in follow.Keys)
            {
                Console.Write($"{i}  ");
                Console.Write(string.Join(" ", follow[i].ToArray()));
                Console.WriteLine();
            }

            List<string> ColumnSymbols = new List<string>(g.Terminals)
            {
                Constants.END
            };
            List<string> RowSymbols = new List<string>(g.NonTerminals);
            RowSymbols.AddRange(g.Terminals);
            RowSymbols.Add(Constants.END);
            var tableColumns = new List<string>
            {
                " "
            };
            tableColumns.AddRange(ColumnSymbols);
            ConsoleTable table = new ConsoleTable(tableColumns.ToArray());
            ParsingTable parsingTable = p.GetParsingTable(first, follow, g);
            foreach (var row in RowSymbols)
            {
                List<string> tableRow = new List<string>
                {
                    row
                };
                foreach (var col in ColumnSymbols)
                {
                    var value = parsingTable.ContainsKeyPair(row, col) ? parsingTable[row, col].ToString() : "";
                    tableRow.Add(value);
                }

                table.AddRow(tableRow.ToArray());
            }

            Console.WriteLine("Parsing Table:");
            table.Write(Format.Alternative);
            StreamReader reader = new StreamReader("inputsequence.txt");
            string inputSequence = reader.ReadToEnd();
            Console.WriteLine("Input Sequence:");
            Console.WriteLine(inputSequence);
            //            var analizer = new Analizer(g, parsingTable, inputSequence);
            //            try
            //            {
            //                Stack<int> output = analizer.FillOutputStack();
            //                var arr = output.ToArray();
            //
            //                Console.WriteLine("Output Stack:");
            //                for (int i = arr.Length - 1; i >= 0; i--)
            //                {
            //                    Console.Write((arr[i] == -1 ? Constants.EPSILON : arr[i].ToString()) + " ");
            //                }
            //
            //                Console.WriteLine();
            //
            //                Console.WriteLine("Parsing Tree: ");
            //                ParsingTree tree = new ParsingTree(output, g);
            //                var startingNode = tree.GenerateParsingTree();
            //                Console.WriteLine(startingNode.ToString());
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine(ex.Message);
            //            }

            Console.WriteLine("\n\n\n\n\n\n\n");
            ArrayList abc = new ArrayList();
            PifGenerator ss = new PifGenerator() { Filename = "inputsequence.txt" };
            ss.Scan();

            foreach (var constantsValue in ss.InternalForm)
            {
                abc.Add(constantsValue.Code);
            }
            string inputSequence2 = null;
            foreach (int i in abc)
            {
                if (i != 9)
                {
                    Console.WriteLine(i);
                    inputSequence2 += i + " ";
                }
            }
            inputSequence2 = inputSequence2.Remove(inputSequence2.Length - 1);

            var analizer2 = new Analizer(g, parsingTable, inputSequence2);
            try
            {
                Stack<int> output = analizer2.FillOutputStack();
                var arr = output.ToArray();

                Console.WriteLine("Output Stack:");
                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    Console.Write((arr[i] == -1 ? Constants.EPSILON : arr[i].ToString()) + " ");
                }

                Console.WriteLine();

                Console.WriteLine("Parsing Tree: ");
                ParsingTree tree = new ParsingTree(output, g);
                var startingNode = tree.GenerateParsingTree();
                Console.WriteLine(startingNode.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
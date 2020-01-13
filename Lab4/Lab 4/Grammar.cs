using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LFTC
{
    public class Grammar
    {
        public LinkedList<string> NonTerminals = new LinkedList<string>();
        public HashSet<string> Terminals = new HashSet<string>();
        public List<Production> Productions = new List<Production>();
        public string StartingSymbol = string.Empty;

        public Production GetProductionByIndex(int index)
        {
            var temp = Productions.First(p => p.Rules.Any(r => r.Index == index));
            return new Production()
            {
                Start = temp.Start,
                Rules = new List<Rule>()
                {
                    temp.Rules.First(r => r.Index == index)
                }
            };
    }
        public static Grammar FromFile(string filename)
        {
            string line;
            StreamReader file = new StreamReader(filename);


            LinkedList<string> nonTerminals = new LinkedList<string>();
            HashSet<string> terminals = new HashSet<string>();
            List<Production> productions = new List<Production>();
            string startingSymbol = string.Empty;
            int i = 0;
            int index = 0;
            while ((line = file.ReadLine()) != null)
            {
                if (i <= 2)
                {
                    string[] tokens = line.Trim().Split(" ");
                    for (int j = 0; j < tokens.Length; j++)
                    {
                        if (i == 0)
                        {
                            nonTerminals.AddLast(tokens[j]);
                        }
                        if (i == 1)
                        {
                            terminals.Add(tokens[j]);
                        }
                        if (i == 2)
                        {
                            startingSymbol = tokens[j];
                        }

                    }
                }

                if (i > 2)
                {
                    string[] tokens = line.Split(" -> ");
                    List<Rule> rules = new List<Rule>();

                    foreach (string rule in tokens[1].Split(" | "))
                    {
                        index++;
                        rules.Add(new Rule(rule.Trim().Split(" "), index));
                    }
                    productions.Add(new Production()
                    {
                        Start = tokens[0],
                        Rules = rules
                    });

                }
                i++;
            }
            return new Grammar()
            {
                Terminals = terminals,
                NonTerminals = nonTerminals,
                StartingSymbol = startingSymbol,
                Productions = productions
            };
        }

        public List<Production> ProductionsForSymbol(string symbol)
        {
            List<Production> ProductionsForSymbol = new List<Production>();
            foreach (Production production in Productions)
            {
                if (production.Start == symbol)

                {
                    ProductionsForSymbol.Add(production);
                }
            }
            return ProductionsForSymbol;
        }

        public List<Production> ProductionsForNonterminal(string nonTerminal)
        {
            List<Production> productionsForNonterminal = new List<Production>();
            foreach (Production production in Productions)
            {
                if (production.Start == nonTerminal)

                {
                    productionsForNonterminal.Add(production);
                }
            }
            return productionsForNonterminal;
        }
    }
    public class Production
    {
        public string Start;
        public List<Rule> Rules;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Start + " -> ");
            foreach (var rule in Rules)
            {
                foreach (string element in rule)
                    sb.Append(element).Append(" ");
                sb.Append("| ");
            }

            sb.Remove(sb.Length - 3, sb.Length - 1);
            return sb.ToString();
        }
    }
    public class Rule
    {
        public List<string> Symbols = new List<string>();
        public int Index { get; set; }
        public Rule(string[] symbols, int index)
        {
            Symbols.AddRange(new List<string>(symbols).Where(symbol => !string.IsNullOrEmpty(symbol)));
            Index = index;
        }
        public IEnumerator<string> GetEnumerator()
        {
            return Symbols.GetEnumerator();
        }
        public string this[int index]
        {
            get
            {
                return Symbols[index];
            }
        }
        public int Count => Symbols.Count;
        public bool Any(Func<string, bool> p) => Symbols.Any(p);
        public int IndexOf(string symbol) => Symbols.IndexOf(symbol);
        public string First() => Symbols.First();

        public override string ToString()
        {
            return string.Join("", Symbols.ToArray()) + (Index > 0 ? "," + Index : "");
        }

        public List<string> SubList(int start, int count)
        {
            return Symbols.Skip(start).Take(count).ToList();
        }
    }
}

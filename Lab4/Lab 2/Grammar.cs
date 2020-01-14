using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab2
{
    internal class Production
    {
        public char RuleName;
        public string Rule;

        public Production(char ruleName, string rule)
        {
            RuleName = ruleName;
            Rule = rule;
        }

        public override string ToString()
        {
            return RuleName + " -> " + Rule;
        }
    }

    internal class Grammar
    {
        public List<string> N;
        public List<string> E;
        public string S;
        public List<Production> P;

        public Grammar(List<string> n, List<string> e, string s, List<Production> p)
        {
            this.N = n;
            this.E = e;
            this.S = s;
            this.P = p;
        }

        public static List<string> ParseLine(string line)
        {
            return new List<string>(line.Trim().Split('=')[1].Trim()[1..^1].Trim().Split(',').Select(i => i.Trim()));
        }

        public static Grammar FromFile(string filename)
        {
            using (StreamReader file = new StreamReader(filename))
            {
                var N = Grammar.ParseLine(file.ReadLine());
                var E = Grammar.ParseLine(file.ReadLine());
                var S = file.ReadLine().Split('=')[1].Trim();
                List<string> lines = new List<string>();
                string line = string.Empty;
                while ((line = file.ReadLine()) != null)
                    lines.Add(line);
                var P = Grammar.ParseRules(Grammar.ParseLine(string.Join("", lines)));
                return new Grammar(N, E, S, P);
            }
        }

        public static Grammar FromFiniteAutomata(FiniteAutomata fa)
        {
            var N = fa.Q;
            var E = fa.E;
            var S = fa.q0;

            var P = new List<Production>();
            if (fa.F.Contains(fa.q0))
                P.Add(new Production(fa.q0[0], "E"));
            foreach (var transition in fa.S)
            {
                var state1 = transition.SourceState;
                var state2 = transition.DestinationState;
                var route = transition.Route;
                P.Add(new Production(state1[0], route + state2));
                if (fa.F.Contains(state2))
                    P.Add(new Production(state1[0], route));
            }

            return new Grammar(N, E, S, P);
        }

        public static List<Production> ParseRules(List<string> rules)
        {
            List<Production> result = new List<Production>();
            foreach (var rule in rules)
            {
                var lhs = rule.Split("->")[0].Trim();
                var values = new List<string>(rule.Split("->")[1].Split('|').Select(i => i.Trim()));
                foreach (var value in values)
                    result.Add(new Production(lhs[0], value));
            }

            return result;
        }

        public bool IsNonTerminal(char value)
        {
            return this.N.Contains(value.ToString());
        }

        public bool IsTerminal(string value)
        {
            return this.E.Contains(value);
        }

        public bool IsRegular()
        {
            var usedInRhs = new Dictionary<char, bool>();
            var notAllowedInRhs = new List<char>();
            foreach (var rule in P)
            {
                var lhs = rule.RuleName;
                var rhs = rule.Rule;
                var hasTerminal = false;
                var hasNonTerminal = false;
                if (rhs.Length > 2)
                {
                    return false;
                }

                foreach (var chr in rhs)
                {
                    if (IsNonTerminal(chr))
                    {
                        usedInRhs[chr] = true;
                        hasNonTerminal = true;
                    }
                    else if (IsTerminal(chr.ToString()))
                    {
                        if (hasNonTerminal)
                            return true;
                        hasTerminal = true;
                    }

                    if (chr == 'E')
                    {
                        notAllowedInRhs.Add(lhs);
                    }
                }

                if (hasNonTerminal && !hasTerminal)
                    return false;
            }

            foreach (var chr in notAllowedInRhs)
                if (usedInRhs.ContainsKey(chr))
                    return false;

            return true;
        }

        public List<Production> GetProductionsFor(char nonTerminal)
        {
            if (!IsNonTerminal(nonTerminal))
                throw new Exception("Can only show productions for non-terminals");
            List<Production> productions = new List<Production>();
            return P.Where(prod => prod.RuleName == nonTerminal).ToList();
        }

        public void ShowProductionsFor(char nonTerminal)
        {
            var productions = GetProductionsFor(nonTerminal);
            Console.WriteLine(string.Join(",", productions.Select(prod => prod.ToString())));
        }

        public override string ToString()
        {
            return "N = { " + string.Join(",", N) + " }\n"
                   + "E = { " + string.Join(",", E) + " }\n"
                   + "P = { " + string.Join(",", P.Select(prod => prod.ToString())) + " }\n"
                   + "S = " + S + "\n";
        }
    }
}
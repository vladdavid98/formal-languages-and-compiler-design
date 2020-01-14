using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab2
{
    internal class Transition
    {
        public string SourceState;
        public string Route;
        public string DestinationState;

        public Transition(string sourceState, string route, string destinationState)
        {
            SourceState = sourceState;
            Route = route;
            DestinationState = destinationState;
        }

        public override string ToString()
        {
            return "( '" + SourceState + "' , '" + Route + "' ) -> " + DestinationState;
        }
    }

    internal class FiniteAutomata
    {
        public List<string> Q;
        public List<string> E;
        public List<Transition> S;
        public string q0;
        public List<string> F;

        public FiniteAutomata(List<string> q, List<string> e, List<Transition> s, string q0, List<string> f)
        {
            this.Q = q;
            this.E = e;
            this.S = s;
            this.q0 = q0;
            this.F = f;
        }

        public static List<string> ParseLine(string line)
        {
            return new List<string>(line.Trim().Split('=')[1].Trim()[1..^1].Trim().Split(',').Select(i => i.Trim()));
        }

        public static FiniteAutomata FromFile(string filename)
        {
            using StreamReader file = new StreamReader(filename);
            var Q = FiniteAutomata.ParseLine(file.ReadLine());
            var E = FiniteAutomata.ParseLine(file.ReadLine());
            var q0 = file.ReadLine().Split('=')[1].Trim();
            var F = FiniteAutomata.ParseLine(file.ReadLine());

            List<string> lines = new List<string>();
            string line = string.Empty;
            while ((line = file.ReadLine()) != null)
                lines.Add(line);
            var S = FiniteAutomata.ParseTransitions(FiniteAutomata.ParseLine(string.Join("", lines)));

            return new FiniteAutomata(Q, E, S, q0, F);
        }

        public static List<Transition> ParseTransitions(List<string> parts)
        {
            List<Transition> result = new List<Transition>();
            List<string> transitions = new List<string>();
            int index = 0;
            while (index < parts.Count)
            {
                transitions.Add($"{parts[index]},{parts[index + 1]}");
                index += 2;
            }

            foreach (var transition in transitions)
            {
                var lhs = transition.Split("->")[0].Trim();
                var rhs = transition.Split("->")[1].Trim();
                var state2 = rhs.Trim();
                var state1 = lhs[1..^1].Split(',')[0];
                var route = lhs[1..^1].Split(',')[1];
                result.Add(new Transition(state1, route, state2));
            }

            return result;
        }

        public static FiniteAutomata FromRegularGramar(Grammar rg)
        {
            var Q = new List<string>(rg.N)
            {
                "K"
            };
            var E = new List<string>(rg.E);
            var q0 = rg.S;
            var F = new List<string>() { "K" };
            var S = new List<Transition>();
            foreach (var prod in rg.P)
            {
                var state2 = "K";
                var state1 = prod.RuleName;
                var rhs = prod.Rule;
                if (state1.ToString() == q0 && rhs == "E")
                {
                    F.Add(q0);
                    continue;
                }

                var route = rhs[0].ToString();
                if (rhs.Length == 2)
                {
                    state2 = rhs[1].ToString();
                }

                S.Add(new Transition(state1.ToString(), route, state2));
            }

            return new FiniteAutomata(Q, E, S, q0, F);
        }

        public bool IsState(string value)
        {
            return Q.Contains(value);
        }

        public List<Transition> GetTransitionsFor(string state)
        {
            if (!IsState(state))
                throw new Exception("Can Only get transition for states");
            List<Transition> transitions = new List<Transition>();
            foreach (var transition in this.S)
                if (transition.SourceState.Contains(state))
                    transitions.Add(transition);
            return transitions;
        }

        public void ShowProductionsFor(string state)
        {
            var transitions = GetTransitionsFor(state);
            Console.WriteLine(string.Join(",", transitions.Select(prod => prod.ToString())));
        }

        public override string ToString()
        {
            return "Q = { " + string.Join(",", Q) + " }\n"
                   + "E = { " + string.Join(",", E) + " }\n"
                   + "F = { " + string.Join(",", F) + " }\n"
                   + "S = { " + string.Join(",", S.Select(prod => prod.ToString())) + " }\n"
                   + "q0 = " + q0 + "\n";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace LFTC
{
    public static class Constants
    {
        public static readonly string EPSILON = "ε";
        public static readonly string POP = "pop";
        public static readonly string ACC = "acc";
        public static readonly string END = "$";
    }

    public enum ElementType
    {
        POP,
        ACC,
        RULE,
        ERR
    }

    public class Set
    {
        private Dictionary<string, HashSet<string>> set = new Dictionary<string, HashSet<string>>();

        public HashSet<string> this[string index]
        {
            get { return set[index]; }
            set { set[index] = value; }
        }

        public IEnumerable<string> Keys => set.Keys;

        public bool ContainsKey(string key) => set.ContainsKey(key);
    }

    public class FirstSet : Set
    {
    }

    public class FollowSet : Set
    {
    }

    public class ParsingTable
    {
        private Dictionary<Tuple<string, string>, Rule> Table = new Dictionary<Tuple<string, string>, Rule>();

        public Rule this[string row, string column]
        {
            get { return Table[new Tuple<string, string>(row, column)]; }
            set { Table[new Tuple<string, string>(row, column)] = value; }
        }

        public bool ContainsKeyPair(string row, string column)
        {
            return Table.ContainsKey(new Tuple<string, string>(row, column));
        }

        public ElementType GetElementType(string row, string col)
        {
            List<string> reserved = new List<string> { Constants.POP, Constants.ACC };
            if (!ContainsKeyPair(row, col))
                return ElementType.ERR;
            Rule rule = this[row, col];
            if (rule.Count > 0 && !reserved.Contains(rule[0]))
                return ElementType.RULE;
            else if (rule.Count > 0)
            {
                return rule[0] == Constants.ACC
                    ? ElementType.ACC
                    : (rule[0] == Constants.POP ? ElementType.POP : ElementType.ERR);
            }

            return ElementType.ERR;
        }
    }

    public class Analizer
    {
        public Grammar Grammar { get; set; }
        public ParsingTable ParsingTable { get; set; }
        public string InputSequence { get; set; }

        private Stack<string> InputStack { get; set; } = new Stack<string>();
        private Stack<string> WorkStack { get; set; } = new Stack<string>();
        private Stack<int> OutputStack { get; set; } = new Stack<int>();

        public Analizer(Grammar grammar, ParsingTable parsingTable, string inputSequence)
        {
            Grammar = grammar;
            ParsingTable = parsingTable;
            InputSequence = inputSequence;
            string[] symbols = inputSequence.Split(" ");

            // Init input stack

            InputStack.Clear();
            InputStack.Push(Constants.END);
            for (int i = symbols.Length - 1; i >= 0; i--)
                InputStack.Push(symbols[i]);
            // Init work stack

            WorkStack.Push(Constants.END);
            WorkStack.Push(grammar.StartingSymbol);

            //Init Output stack

            OutputStack.Push(-1);
        }

        public Stack<int> FillOutputStack()
        {
            while (true)
            {
                var wsp = WorkStack.Peek();
                var isp = InputStack.Peek();
                var type = ParsingTable.GetElementType(WorkStack.Peek(), InputStack.Peek());
                switch (type)
                {
                    case ElementType.RULE:
                        var rule = ParsingTable[WorkStack.Peek(), InputStack.Peek()];
                        WorkStack.Pop();
                        if (rule[0] != Constants.EPSILON)
                            for (int i = rule.Count - 1; i >= 0; i--)
                                WorkStack.Push(rule[i]);
                        OutputStack.Push(rule.Index);
                        break;

                    case ElementType.POP:
                        WorkStack.Pop();
                        InputStack.Pop();
                        break;

                    case ElementType.ACC:
                        return OutputStack;

                    case ElementType.ERR:
                        throw new Exception("Invalid Input sequence");
                }
            }
        }
    }
}
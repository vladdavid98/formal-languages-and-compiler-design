using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LFTC
{
    public class ParsingTree
    {
        private readonly Grammar _grammar;
        private readonly Stack<int> _outputStack;

        public ParsingTree(Stack<int> outputStack, Grammar g)
        {
            _grammar = g;
            _outputStack = outputStack;
        }

        public ParsingNode GenerateParsingTree()
        {
            var arr = _outputStack.ToArray();
            ParsingNode root = null;
            ParsingNode current = null;
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (arr[i] == -1)
                {
                    root = new ParsingNode()
                    {
                        ProductionIndex = arr[i - 1],
                        Symbols = new List<string> { _grammar.StartingSymbol }
                    };
                    current = root;
                }
                else
                {
                    var prod = _grammar.GetProductionByIndex(arr[i]);
                    int symbolIndex = current.Symbols.IndexOf(prod.Start);
                    List<string> symbols = new List<string>();
                    symbols.AddRange(current.Symbols.Take(symbolIndex).ToList());
                    symbols.AddRange(prod.Rules[0].Symbols);
                    symbols.AddRange(current.Symbols.Skip(symbolIndex + 1));
                    var newNode = new ParsingNode()
                    {
                        ProductionIndex = arr[Math.Max(i - 1, 0)],
                        Symbols = symbols
                    };
                    current.Child = newNode;
                    current = newNode;
                }
            }

            return root;
        }
    }

    public class ParsingNode
    {
        public ParsingNode Child { get; set; }
        public int ProductionIndex = 0;
        public List<string> Symbols = new List<string>();

        public override string ToString()
        {
            if (Child != null)
                return $"({string.Join("", Symbols.ToArray())},{ProductionIndex}) -> {Child.ToString()}";
            else
                return $"{string.Join("", Symbols.ToArray())}";
        }
    }
}
using ConsoleTables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lab1
{
    internal class LanguageSpecification
    {
        public static List<string> separators = new List<string>() { "[", "]", "{", "}", "(", ")", ";", " ", ":" };

        public static List<string> operators = new List<string>()
        {
            "+", "-", "*", "/", "%", "<", "<=", "=", ">=", ">",
            ">>", "<<", "==", "&&", "||", "!", "!=", "&", "~",
            "|", "^", "++", "--"
        };

        public static List<string> reservedWords = new List<string>()
        {
            "break", "case", "char", "const", "continue", "default", "do", "double", "else", "float", "for", "cout",
            "if", "int", "long", "return", "short", "sizeof", "switch", "void", "while"
        };

        private static Dictionary<string, int> codification;

        public static Dictionary<string, int> Codification
        {
            get
            {
                if (codification == null)
                {
                    int i = 2;
                    codification = new Dictionary<string, int>();
                    separators.ForEach(s =>
                    {
                        codification[s] = i;
                        i++;
                    });
                    operators.ForEach(s =>
                    {
                        codification[s] = i;
                        i++;
                    });
                    reservedWords.ForEach(s =>
                    {
                        codification[s] = i;
                        i++;
                    });
                    codification["identifier"] = 0;
                    codification["constant"] = 1;
                }

                return codification;
            }
        }
    }

    internal class Scanner
    {
        public static bool IsIdentifier(string token)
        {
            return Regex.Match(token, "^[a-zA-Z][a-zA-Z0-9_]{0,249}$").Success;
        }

        public static bool IsConstant(string token)
        {
            if (Regex.Match(token, @"^\'[a-zA-Z0-9]\'$").Success)
                return true;
            if (Regex.Match(token, @"^[0]|^[+_]{0,1}[1-9][0-9]*$").Success)
                return true;

            if (Regex.Match(token, @"^[0]|^[+_]{0,1}[1-9][0-9]*[,]{0,1}[0-9]{1,}$").Success)
                return true;
            return false;
        }

        public static bool IsPartOfOperator(char character)
        {
            foreach (var op in LanguageSpecification.operators)
            {
                if (op.Contains(character))
                    return true;
            }

            return false;
        }

        public static bool IsEscapedQuote(string line, int index)
        {
            return index != 0 && (line[index - 1] == '\\');
        }

        public static Tuple<string, int> GetStringToken(string line, int index)
        {
            string token = string.Empty;
            int quoteCount = 0;
            while (index < line.Length && quoteCount < 2)
            {
                if (line[index] == '"' && !IsEscapedQuote(line, index))
                    quoteCount += 1;
                token += line[index];
                index += 1;
            }

            return new Tuple<string, int>(token, index);
        }

        public static Tuple<string, int> GetOperatorToken(string line, int index)
        {
            string token = string.Empty;
            while (index < line.Length && IsPartOfOperator(line[index]))
            {
                token += line[index];
                index += 1;
            }

            return new Tuple<string, int>(token, index);
        }

        public static List<string> TokenGenerator(string line)
        {
            string token = string.Empty;
            List<string> tokens = new List<string>();
            for (int index = 0; index < line.Length; index++)
            {
                if (IsPartOfOperator(line[index]))
                {
                    if (token.Length > 0)
                        tokens.Add(token);
                    var pair = GetOperatorToken(line, index);
                    token = pair.Item1;
                    index = pair.Item2;
                    token = Regex.Replace(token, @"\s+", "");
                    tokens.Add(token);
                    token = string.Empty;
                }
                else if (LanguageSpecification.separators.Contains(line[index].ToString()))
                {
                    if (token.Length > 0)
                    {
                        token = Regex.Replace(token, @"\s+", "");
                        tokens.Add(token);
                    }

                    token = line[index].ToString();
                    tokens.Add(token);
                    token = string.Empty;
                }
                else
                {
                    token += line[index].ToString();
                }
            }

            if (token.Length > 0)
            {
                token = Regex.Replace(token, @"\s+", "");
                tokens.Add(token);
            }

            return tokens;
        }
    }

    internal class InternalFormItem
    {
        public int Code;
        public int Id;
    }

    internal class Program
    {
        public string Filename = string.Empty;
        public Hashtable Symbols = new Hashtable();
        public Hashtable Constants = new Hashtable();
        public List<InternalFormItem> InternalForm = new List<InternalFormItem>();

        public void Scan()
        {
            StreamReader file = new StreamReader(Filename);
            List<string> lines = new List<string>();
            {
                string line = string.Empty;
                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                    Console.WriteLine(line);
                }
            }
            int id = 0;
            file.Close();
            int lineNo = 0;
            foreach (var line in lines)
            {
                lineNo++;
                foreach (var token in Scanner.TokenGenerator(line))
                {
                    if (LanguageSpecification.Codification.ContainsKey(token))
                    {
                        InternalForm.Add(new InternalFormItem()
                        { Code = LanguageSpecification.Codification[token], Id = -1 });
                    }
                    else if (Scanner.IsIdentifier(token))
                    {
                        if (Symbols.Values.OfType<string>().All(i => i != token))
                        {
                            Symbols.Add(id, token);
                            InternalForm.Add(new InternalFormItem()
                            { Code = LanguageSpecification.Codification["identifier"], Id = id.GetHashCode() });
                            id++;
                        }
                    }
                    else if (Scanner.IsConstant(token))
                    {
                        if (Constants.Values.OfType<string>().All(i => i != token))
                        {
                            Constants.Add(id, token);
                            InternalForm.Add(new InternalFormItem()
                            { Code = LanguageSpecification.Codification["constant"], Id = id.GetHashCode() });
                            id++;
                        }
                    }
                    else
                        throw new Exception("Unknown token " + token + " at line " + lineNo);
                }
            }

            ConsoleTable internalForm = new ConsoleTable("Code", "ID");
            InternalForm.ForEach(i => internalForm.AddRow(i.Code, i.Id));
            Console.WriteLine("\nProgram Internal Form:");
            internalForm.Write();
            ConsoleTable symbols = new ConsoleTable("Token", "ID");
            foreach (var k in Symbols.Keys)
                symbols.AddRow(Symbols[k], k.GetHashCode());
            Console.WriteLine("\nSymbols:");
            symbols.Write();
            ConsoleTable constants = new ConsoleTable("Token", "ID");
            foreach (var k in Constants.Keys)
                constants.AddRow(Constants[k], k.GetHashCode());
            Console.WriteLine("\nConstants:");
            constants.Write();
        }

        private static void Main(string[] args)
        {
            Program p = new Program() { Filename = "program.txt" };
            try
            {
                p.Scan();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
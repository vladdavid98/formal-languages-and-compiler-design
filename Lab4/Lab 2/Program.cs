using System;
using System.Collections.Generic;
using System.IO;

namespace Lab2
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Grammar usage
            // We read the grammar from file 'rg1.txt'

            Console.WriteLine("Grammar");
            var grammar = Grammar.FromFile("rg1.txt");
            Console.WriteLine(grammar.ToString());

            // We print the productions for a given non-terminal,
            // A in this case
            try
            {
                Console.WriteLine("Productions for S");
                grammar.ShowProductionsFor('S');
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // Finite Automata usage
            // We read the Finite Automata from the file 'fa1.txt'

            var finiteAutomata = FiniteAutomata.FromFile("fa1.txt");
            Console.WriteLine("Finite Automata");
            Console.WriteLine(finiteAutomata);

            // Transformations

            Console.WriteLine("Regular Grammar -> Finite Automata");

            grammar = Grammar.FromFile("rg1.txt");
            if (grammar.IsRegular())
            {
                finiteAutomata = FiniteAutomata.FromRegularGramar(grammar);
                Console.WriteLine(finiteAutomata);
            }
            else
                Console.WriteLine("The grammar is not regular\n");

            Console.WriteLine("Finite Automata -> Regular Grammar");

            finiteAutomata = FiniteAutomata.FromFile("fa1.txt");
            grammar = Grammar.FromFiniteAutomata(finiteAutomata);
            Console.WriteLine(grammar);
        }
    }
}
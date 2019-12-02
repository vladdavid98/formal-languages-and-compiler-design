from model.finiteAutomata import FiniteAutomata
from model.grammar import Grammar

if __name__ == '__main__':

    # Read grammar from file
    grammar = Grammar.fromFile('rg1.txt')
    print(grammar)

    # Print productions for non-terminal
    try:
        grammar.showProductionsFor('S')
    except Exception as e:
        print(e)

    # Read grammar from console
    # grammar2 = Grammar.fromConsole()
    # print('\n' + str(grammar2))

    # Read FA from file
    finiteAutomata = FiniteAutomata.fromFile('fa1.txt')
    print(finiteAutomata)

    # Read FA from console
    # finiteAutomata2 = FiniteAutomata.fromConsole()
    # print('\n' + str(finiteAutomata2))

    # Regular Grammar -> Finite Automata
    grammar = Grammar.fromFile('rg1.txt')
    if grammar.isRegular():
        finiteAutomata = FiniteAutomata.fromRegularGrammar(grammar)
        print(finiteAutomata)
    else:
        print("The grammar is not regular\n")

    # Finite Automata -> Regular Grammar
    finiteAutomata = FiniteAutomata.fromFile('fa1.txt')
    grammar = Grammar.fromFiniteAutomata(finiteAutomata)

    print(grammar)

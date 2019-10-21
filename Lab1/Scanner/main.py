from model.my_language_specification import *
from model.pif import ProgramInternalForm
from model.scanner import tokenGenerator, isIdentifier, isConstant
from model.st import SymbolTable

if __name__ == '__main__':
    fileName = input("Insert file name: ")

    file = open(fileName, 'r')
    for line in file:
        print(line)

    with open(fileName, 'r') as file:
        for line in file:
            print([token for token in tokenGenerator(line, separators)])

    symbolTable = SymbolTable()
    pif = ProgramInternalForm()

    with open(fileName, 'r') as file:
        lineNo = 0
        for line in file:
            lineNo += 1
            if line[-1]=='\n':
                line=line[0:-1]
            for token in tokenGenerator(line, separators):
                if token in separators + operators + reservedWords:
                    pif.add(codification[token], -1)
                elif isIdentifier(token):
                    id = symbolTable.add(token)
                    pif.add(codification['identifier'], id)
                elif isConstant(token):
                    id = symbolTable.add(token)
                    pif.add(codification['constant'], id)
                else:
                    raise Exception('Unknown token ' + token + ' at line ' + str(lineNo))

    print('Program Internal Form: \n', pif)
    print('Symbol Table: \n', symbolTable)

    print('\n\nCodification table: ')
    for e in codification:
        print(e, " -> ", codification[e])

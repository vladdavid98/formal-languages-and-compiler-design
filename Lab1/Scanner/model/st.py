from model.binaryTree import Tree


class SymbolTable:
    def __init__(self):
        self.__tree = Tree()

    def add(self, value):
        return self.__tree.add(value)

    def get(self, value):
        return self.__tree.find(value)

    def __str__(self):
        return str(self.__tree)

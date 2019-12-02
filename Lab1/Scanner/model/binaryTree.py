class Node:
    def __init__(self, val, count):
        self.left = None
        self.right = None
        self.value = val
        self.count = count


class Tree:
    def __init__(self):
        self.root = None
        self.__count = 0

    def getRoot(self):
        return self.root

    def add(self, val):
        if self.root is None:
            self.root = Node(val, self.__count)
            self.__count += 1
        else:
            self._add(val, self.root)
        return self.__count - 1

    def _add(self, val, node):
        if val < node.value:
            if node.left is not None:
                self._add(val, node.left)
            else:
                node.left = Node(val, self.__count)
                self.__count += 1
        else:
            if node.right is not None:
                self._add(val, node.right)
            else:
                node.right = Node(val, self.__count)
                self.__count += 1

    def find(self, val):
        if self.root is not None:
            return self._find(val, self.root)
        else:
            return None

    def _find(self, val, node):
        if val == node.value:
            return node
        elif val < node.value and node.left is not None:
            self._find(val, node.left)
        elif val > node.value and node.right is not None:
            self._find(val, node.right)

    def deleteTree(self):
        # garbage collector will do this for us.
        self.root = None

    def __str__(self):
        s = ""
        if self.root is not None:
            s += self._printTree(self.root)
        return s

    def _printTree(self, node):
        s = ''
        if node is not None:
            s += '('
            s += '\''
            s += str(node.value)
            s += '\', '
            s += str(node.count)
            s += ')'
            s += ', '
            s += (self._printTree(node.left))
            s += (self._printTree(node.right))
        return s

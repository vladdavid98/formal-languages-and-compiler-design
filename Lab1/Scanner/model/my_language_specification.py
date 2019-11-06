separators = ['[', ']', '{', '}', '(', ')', ';', ' ', ':']
operators = ['+', '-', '*', '/', '%', '<', '<=', '=', '>=', '>',
             '>>', '<<', '==', '&&', '||', '!', '!=', '&', '~',
             '|', '^', '++', '--', ',']
reservedWords = ['opreste', 'caz', 'caracter', 'constanta', 'continua',
                 'implicit', 'fa', 'dublu', 'altfel', 'pluteste', 'pentru', 'printeaza',
                 'daca', 'numar', 'lung', 'intoarce', 'scurt', 'marime', 'static', 'intrerupator',
                 'definire', 'vid', 'cattimp', 'inlinie']

everything = separators + operators + reservedWords
codification = dict([(everything[i], i + 2) for i in range(len(everything))])
codification['identifier'] = 0
codification['constant'] = 1

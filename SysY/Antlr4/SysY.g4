grammar SysY;

compilationUnit
    : (globalValues+=declaration | functions+=functionDefinition)+ EOF
    ;
    
declaration
    : variableDeclaration
    | constantDeclaration
    ;
    
bType
    : 'int'
    | 'float'
    ;
    
constantDeclaration
    : 'const' bType constants+=constDefinition (',' constants+=constDefinition)* ';'
    ;
    
constDefinition
    : Identifier ('[' constExpression ']')? '=' constInitValue ';'
    ;
    
constInitValue
    : constExpression
    | '{' elements+=constInitValue (',' elements+=constInitValue)* '}'
    ;
    
variableDeclaration
    : bType variables+=variableDefinition (',' variables+=variableDefinition)* ';'
    ;
    
variableDefinition
    : Identifier ('[' constExpression ']')? ('=' initialValue)? ';' #Definition
    | Identifier ('[' constExpression ']')? #Declare
    ;
    
initialValue
    : expression
    | '{' elements+=initialValue (',' elements+=initialValue)* '}'
    ;
    
functionDefinition
    : functionType Identifier '(' (parameters+=parameterDeclaration (',' parameters+=parameterDeclaration)*)? ')' block
    ;
    
functionType
    : bType
    | 'void'
    ;
    
parameterDeclaration
    : bType Identifier ('[' ']' ('[' expression ']')*)?
    ;
    
block
    : '{' (statements+=blockItem)* '}'
    ;
    
blockItem
    : statement
    | declaration
    ;
    
statement
    : leftValue '=' expression ';' #Assignment
    | expression? ';' #NormalExpression
    | 'if' '(' condition=logicalOrExpression ')' trueBlock=statement ('else' falseBlock=statement)? #If
    | 'while' '(' condition=logicalOrExpression ')' loopBody=statement #While
    | 'break' ';' #Break
    | 'continue' ';' #Continue
    | 'return' expression? ';' #Return
    ;
    
expression
    : additiveExpression
    ;
    
leftValue
    : Identifier ('[' expression ']')* #ArrayAccess
    | Identifier #VariableAccess
    ;
    
primaryExpression
    : leftValue #LeftValueAccess
    | Constant #Constant
    | '(' expression ')' #Parentheses
    ;

unaryExpression
    : '+' expression #UnaryPlus
    | '-' expression #UnaryMinus
    | '!' expression #LogicalNot
    | primaryExpression #Primary
    | Identifier '(' (arguments+=expression (',' arguments+=expression)*)? ')' #FunctionCall
    ;
    
multiplicativeExpression
    : unaryExpression
    | multiplicativeExpression ('*' | '/' | '%') unaryExpression
    ;
    
additiveExpression
    : multiplicativeExpression
    | additiveExpression ('+' | '-') multiplicativeExpression
    ;
    
relationalExpression
    : additiveExpression
    | relationalExpression ('<' | '>' | '<=' | '>=') additiveExpression
    ;
    
equalityExpression
    : relationalExpression
    | equalityExpression ('==' | '!=') relationalExpression
    ;
    
logicalAndExpression
    : equalityExpression
    | logicalAndExpression '&&' equalityExpression
    ;
    
logicalOrExpression
    : logicalAndExpression
    | logicalOrExpression '||' logicalAndExpression
    ;   
    
constExpression
    : additiveExpression
    ;

SingleLineComment
    : '//' ~[\r\n]* -> skip
    ;
    
MultiLineComment
    : '/*' .*? '*/' -> skip
    ;

Identifier
    : [a-zA-Z_][a-zA-Z0-9_]* // Identifiers start with a letter or underscore, followed by letters, digits, or underscores
    ;
    
// Copy From C.g4 https://github.com/antlr/grammars-v4/blob/master/c/C.g4
Constant
    : IntegerConstant
    | FloatingConstant
    ;
    
fragment IntegerConstant
    : Sign DecimalConstant
    | OctalConstant
    | HexadecimalConstant
    | BinaryConstant
    ;
    
fragment Sign
    : '+' | '-'
    ;

fragment BinaryConstant
    : '0' [bB] [0-1]+
    ;

fragment DecimalConstant
    : NonzeroDigit Digit*
    ;

fragment OctalConstant
    : '0' OctalDigit*
    ;

fragment HexadecimalConstant
    : HexadecimalPrefix HexadecimalDigit+
    ;
    
fragment HexadecimalPrefix
    : '0' [xX]
    ;

fragment Digit
    : [0-9]
    ;

fragment NonzeroDigit
    : [1-9]
    ;

fragment OctalDigit
    : [0-7]
    ;

fragment HexadecimalDigit
    : [0-9a-fA-F]
    ;
    
fragment FloatingConstant
    : DecimalFloatingConstant
    ;

fragment DecimalFloatingConstant
    : FractionalConstant ExponentPart?
    | DigitSequence ExponentPart
    ;
    
fragment FractionalConstant
    : DigitSequence? '.' DigitSequence
    | DigitSequence '.'
    ;
    
fragment ExponentPart
    : [eE] Sign? DigitSequence
    ;

DigitSequence
    : Digit+
    ;
grammar SysY;

compilationUnit
    : (globalValues+=declaration | functions+=functionDefinition)+ EOF
    ;
    
declaration
    : variableDeclaration
    | constantDeclaration
    ;
    
bType
    : Int
    | Float
    ;
    
constantDeclaration
    : Const bType constants+=constDefinition (Comma constants+=constDefinition)* Semicolon
    ;
    
constDefinition
    : Identifier (LeftBracket dimensions+=constExpression RightBracket)* Assign constInitValue Semicolon
    ;
    
constInitValue
    : constExpression
    | LeftBrace (elements+=constInitValue (Comma elements+=constInitValue)*)? RightBrace
    ;
    
variableDeclaration
    : bType variables+=variableDefinition (Comma variables+=variableDefinition)* Semicolon
    ;
    
variableDefinition
    : Identifier (LeftBracket dimensions+=constExpression RightBracket)? (Assign initialValue)? Semicolon #Definition
    | Identifier (LeftBracket dimensions+=constExpression RightBracket)? #Declare
    ;
    
initialValue
    : expression
    | LeftBrace (elements+=initialValue (Comma elements+=initialValue)*)? RightBrace
    ;
    
functionDefinition
    : functionType Identifier LeftBrace (parameters+=parameterDeclaration (Comma parameters+=parameterDeclaration)*)? RightBrace block
    ;
    
functionType
    : bType
    | Void
    ;
    
parameterDeclaration
    : bType Identifier (LeftBracket RightBracket (LeftBracket expression RightBracket)*)?
    ;
    
block
    : LeftBrace (statements+=blockItem)* RightBrace
    ;
    
blockItem
    : statement
    | declaration
    ;
    
statement
    : leftValue Assign expression Semicolon #Assignment
    | expression? Semicolon #NormalExpression
    | If LeftParenthesis condition=logicalOrExpression RightParenthesis trueBlock=statement (Else falseBlock=statement)? #If
    | While LeftParenthesis condition=logicalOrExpression RightParenthesis loopBody=statement #While
    | Break Semicolon #Break
    | Continue Semicolon #Continue
    | Return expression? Semicolon #Return
    ;
    
expression
    : additiveExpression
    ;
    
leftValue
    : Identifier (LeftBracket expression RightBracket)* #ArrayAccess
    | Identifier #VariableAccess
    ;
    
primaryExpression
    : leftValue #LeftValueAccess
    | Constant #Constant
    | LeftParenthesis expression RightParenthesis #Parentheses
    ;

unaryExpression
    : Plus expression #UnaryPlus
    | Minus expression #UnaryMinus
    | Not expression #UnaryNot
    | primaryExpression #Primary
    | Identifier LeftParenthesis (arguments+=expression (Comma arguments+=expression)*)? RightParenthesis #FunctionCall
    ;
    
multiplicativeExpression
    : unaryExpression
    | multiplicativeExpression (Multiply | Divide | Mod) unaryExpression
    ;
    
additiveExpression
    : multiplicativeExpression
    | additiveExpression (Plus | Minus) multiplicativeExpression
    ;
    
relationalExpression
    : additiveExpression
    | relationalExpression (Less | Greater | LessEqual | GreaterEqual) additiveExpression
    ;
    
equalityExpression
    : relationalExpression
    | equalityExpression (Equal | NotEqual) relationalExpression
    ;
    
logicalAndExpression
    : equalityExpression
    | logicalAndExpression AndAnd equalityExpression
    ;
    
logicalOrExpression
    : logicalAndExpression
    | logicalOrExpression OrOr logicalAndExpression
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
    
// 符号
// =
Assign
    : '='
    ;
    
// + - * / %
Plus
    : '+'
    ;
    
Minus
    : '-'
    ;
    
Multiply
    : '*'
    ;
    
Divide
    : '/'
    ;
    
Mod
    : '%'
    ;
    
// && || ! == != < > <= >=
AndAnd
    : '&&'
    ;
    
OrOr
    : '||'
    ;
    
Not
    : '!'
    ;
    
Equal
    : '=='
    ;
    
NotEqual
    : '!='
    ;
    
Less
    : '<'
    ;
    
Greater
    : '>'
    ;
    
LessEqual
    : '<='
    ;
    
GreaterEqual
    : '>='
    ;
    
// ( ) [ ] { }
LeftParenthesis
    : '('
    ;
    
RightParenthesis
    : ')'
    ;
    
LeftBracket
    : '['
    ;
    
RightBracket    
    : ']'
    ;
    
LeftBrace
    : '{'
    ;
    
RightBrace
    : '}'
    ;
    
// 基本类型 关键字
Int
    : 'int'
    ;
    
Float
    : 'float'
    ;
    
Void
    : 'void'
    ;
    
Const
    : 'const'
    ;
    
Break
    : 'break'
    ;
    
Continue
    : 'continue'
    ;
    
Return
    : 'return'
    ;
    
If
    : 'if'
    ;
    
Else
    : 'else'
    ;
   
While
    : 'while'
    ;
    
// , ;
Comma
    : ','
    ;
    
Semicolon
    : ';'
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
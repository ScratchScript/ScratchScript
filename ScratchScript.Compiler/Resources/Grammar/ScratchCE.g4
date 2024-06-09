grammar ScratchCE;

program: parameter*? expression EOF;

constant: Number | String;
parameter: Identifier '=' constant;

expression
    : constant #constantExpression
    | Identifier #identifierExpression
    | '(' expression ')' #parenthesizedExpression
    | addOperators expression expression #binaryAddExpression
    | multiplyOperators expression expression #binaryMultiplyExpression
    | booleanOperators expression expression #binaryBooleanExpression
    | compareOperators expression expression #binaryCompareExpression
    | '!' expression #notExpression;

addOperators: '+' | '-' | '~';
multiplyOperators: '*' | '/' | '%';

booleanOperators: '&&' | '||' | '^';
compareOperators: '==' | '!=' | '>' | '>=' | '<' | '<=';

fragment Digit: [0-9];
Minus: '-';
Whitespace: (' ' | '\t') -> channel(HIDDEN);
NewLine: ('\r'? '\n' | '\r' | '\n') -> skip;
Number: Minus? Digit+ ([.] Digit+)?; 
Identifier: [a-zA-Z_][a-zA-Z0-9_]*;
String: ('"' (~('"' | '\\' | '\r' | '\n') | '\\' ('"' | '\\'))* '"') | ('\'' (~('\'' | '\\' | '\r' | '\n') | '\\' ('\'' | '\\'))* '\'');
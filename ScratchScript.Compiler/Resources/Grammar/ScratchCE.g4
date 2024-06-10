﻿grammar ScratchCE;

program: parameter*? expression EOF;

constant: Number | String | Boolean;
parameter: Identifier '=' constant;

expression
    : constant #constantExpression
    | Identifier #identifierExpression
    | '(' expression ')' #parenthesizedExpression
    | addOperators expression expression #binaryAddExpression
    | multiplyOperators expression expression #binaryMultiplyExpression
    | bitwiseOperators expression expression #binaryBitwiseExpression
    | booleanOperators expression expression #binaryBooleanExpression
    | compareOperators expression expression #binaryCompareExpression
    | '!' expression #notExpression;

addOperators: '+' | '-' | '~';
multiplyOperators: '*' | '/' | '%';

booleanOperators: '&&' | '||';
compareOperators: '==' | '!=' | '>' | '>=' | '<' | '<=';
leftShift: first='<' second='<' {$first.index + 1 == $second.index}?;
rightShift: first='>' second='>' {$first.index + 1 == $second.index}?;
bitwiseOperators: '|' | '^' | '&' | leftShift | rightShift;

fragment Digit: [0-9];
Minus: '-';
Boolean: 'true' | 'false';
Whitespace: (' ' | '\t') -> channel(HIDDEN);
NewLine: ('\r'? '\n' | '\r' | '\n') -> skip;
Number: Minus? Digit+ ([.] Digit+)?; 
Identifier: [a-zA-Z_][a-zA-Z0-9_]*;
String: ('"' (~('"' | '\\' | '\r' | '\n') | '\\' ('"' | '\\'))* '"') | ('\'' (~('\'' | '\\' | '\r' | '\n') | '\\' ('\'' | '\\'))* '\'');
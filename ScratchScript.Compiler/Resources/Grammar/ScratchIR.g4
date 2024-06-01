grammar ScratchIR;

program: block* EOF;

command
    // Variables
    : 'set' variableIdentifier expression #setCommand
    | 'load' Type Identifier #loadCommand
    
    // Control flow
    | 'while' expression command*? End #whileCommand
    | 'repeat' expression command*? End #repeatCommand
    | ifStatement #ifCommand
    
    // Functions
    | 'call' Identifier #callCommand
    | 'raw' Identifier callFunctionArgument*? #rawCommand
    
    // Lists
    | 'push' Identifier expression #pushCommand
    | 'pushat' Identifier expression expression #pushAtCommand
    | 'pop' Identifier #popCommand
    | 'popat' Identifier expression #popAtCommand
    | 'popall' Identifier #popAllCommand;     
 
block
    : 'block' WarpIdentifier? Identifier command*? End #functionBlock
    | 'on' Event command*? End #eventBlock
    | 'flag' Identifier #flagTopLevelStatement
    | 'define' DefineType Identifier constant*? #defineStatement;

expression
    : constant #constantExpression
    | variableIdentifier #variableExpression
    | arrayIdentifier #arrayExpression
    | argumentIdentifier # argumentExpression
    | '(' expression ')' #parenthesizedExpression
    | addOperators expression expression #binaryAddExpression
    | multiplyOperators expression expression #binaryMultiplyExpression
    | booleanOperators expression expression #binaryBooleanExpression
    | compareOperators expression expression #binaryCompareExpression
    | 'rawshadow' Identifier callFunctionArgument*? 'endshadow' #rawShadowExpression
    | '!' expression #notExpression
    | Identifier '#' expression #listAccessExpression;

Event: 'start'; //todo: add other events
//StopType: 'script'; //todo: add other types

elseIfStatement: 'else' command*? End | ifStatement;
ifStatement: 'if' expression command*? End elseIfStatement?;

callFunctionArgument: functionArgumentType Identifier ':' expression;
functionArgumentType: 'i:' | 'f:';

variableIdentifier: 'var:' Identifier;
arrayIdentifier: 'arr:' Identifier;
argumentIdentifier: 'arg:' Identifier;
constant: Number | String | Color;

addOperators: '+' | '-' | '~';
multiplyOperators: '*' | '/' | '%';

booleanOperators: '&&' | '||' | '^';
compareOperators: '==' | '!=' | '>' | '>=' | '<' | '<=';

DefineType: 'var' | 'list';
Type: NumberType | StringType | ListType;
NumberType: ':number';
StringType: ':string';
ListType: ':list';
StringNumberType: ':sn';
BooleanType: ':boolean';
WarpIdentifier: ':w';

Hashtag: '#';
Minus: '-';
Colon: ':';

End: 'end';

fragment Digit: [0-9];
fragment HexDigit: [0-9a-fA-F];
Whitespace: (' ' | '\t') -> channel(HIDDEN);
NewLine: ('\r'? '\n' | '\r' | '\n') -> skip;
Number: (Minus)? Digit+ ([.] Digit+)?; 
Identifier: [a-zA-Z_][a-zA-Z0-9_]*;
String: ('"' (~('"' | '\\' | '\r' | '\n') | '\\' ('"' | '\\'))* '"') | ('\'' (~('\'' | '\\' | '\r' | '\n') | '\\' ('\'' | '\\'))* '\'');
Color: Hashtag HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit;
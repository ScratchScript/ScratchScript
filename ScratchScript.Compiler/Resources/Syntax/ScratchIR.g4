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
    
    // Procedures
    | 'call' Identifier #callCommand
    | 'raw' Identifier callProcedureArgument*? #rawCommand
    
    // Lists
    | 'push' Identifier expression #pushCommand
    | 'pushat' Identifier expression expression #pushAtCommand
    | 'pop' Identifier #popCommand
    | 'popat' Identifier expression #popAtCommand
    | 'popall' Identifier #popAllCommand;     
 
block
    : 'proc' WarpIdentifier? Identifier procedureArgument* command*? End #procedureBlock
    | 'on' Event command*? End #eventBlock
    | 'flag' Identifier #flagTopLevelStatement;

expression
    : constant #constantExpression
    | variableIdentifier #variableExpression
    | StackIndexIdentifier #stackIndexExpression
    | arrayIdentifier #arrayExpression
    | '(' expression ')' #parenthesizedExpression
    | addOperators expression expression #binaryAddExpression
    | multiplyOperators expression expression #binaryMultiplyExpression
    | booleanOperators expression expression #binaryBooleanExpression
    | compareOperators expression expression #binaryCompareExpression
    | 'rawshadow' Identifier callProcedureArgument*? 'endshadow' #rawShadowExpression
    | '!' expression #notExpression
    | Identifier '#' expression #listAccessExpression;

Event: 'start'; //todo: add other events
//StopType: 'script'; //todo: add other types

elseIfStatement: command*? End | ifStatement;
ifStatement: 'if' expression command*? End ('else' elseIfStatement)?;

procedureArgument: Identifier procedureArgumentTypeDeclaration;
callProcedureArgument: procedureArgumentType Identifier ':' expression;

procedureArgumentType: 'i:' | 'f:';

variableIdentifier: 'var:' Identifier;
arrayIdentifier: 'arr:' Identifier;
constant: Number | String | Color;
procedureArgumentTypeDeclaration: ProcedureType;

addOperators: '+' | '-' | '~';
multiplyOperators: '*' | '/' | '%';

booleanOperators: '&&' | '||' | '^';
compareOperators: '==' | '!=' | '>' | '>=' | '<' | '<=';

Type: NumberType | StringType | ListType;
ProcedureType: StringNumberType | BooleanType;
NumberType: ':number';
StringType: ':string';
ListType: ':list';
StringNumberType: ':sn';
BooleanType: ':boolean';
WarpIdentifier: ':w';
StackIndexIdentifier: ':si:';

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
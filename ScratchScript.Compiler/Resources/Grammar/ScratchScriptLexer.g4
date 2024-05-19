/*
    Lexer fragments
*/
lexer grammar ScratchScriptLexer;

fragment Digit: [0-9];
fragment HexDigit: [0-9a-fA-F];
Whitespace: (' ' | '\t') -> channel(HIDDEN);
NewLine: ('\r'? '\n' | '\r' | '\n') -> skip;
Semicolon: ';';
LeftParen: '(';
RightParen: ')';
LeftBracket: '[';
RightBracket: ']';

LeftBrace: '{' -> pushMode(DEFAULT_MODE);
RightBrace: '}' -> popMode;

OpenInterpolatedString: '`' -> pushMode(STRING_MODE);

Assignment: '=';
Comma: ',';
Not: '!';
Arrow: '->';
Colon: ':';
Dot: '.';
Ternary: '?';

SingleLineCommentStart: '//';
MultiLineCommentStart: '/*';
MultiLineCommentEnd: '*/';

Comment: MultiLineCommentStart .*? MultiLineCommentEnd -> skip;
LineComment: SingleLineCommentStart ~[\r\n]* -> skip;

At: '@';
Hashtag: '#';

Multiply: '*';
Plus: '+';
Minus: '-';
Divide: '/';
Power: '**';
Modulus: '%';

And: '&&';
Or: '||';

BitwiseAnd: '&';
BitwiseOr: '|';
BitwiseXor: '^';

PostIncrement: '++';
PostDecrement: '--';

//<assoc=right>
Greater: '>';
Lesser: '<';
GreaterOrEqual: '>=';
LesserOrEqual: '<=';
Equal: '==';
NotEqual: '!=';

AdditionAsignment: '+=';
SubtractionAssignment: '-=';
MultiplicationAssignment: '*=';
DivisionAssignment: '/=';
ModulusAssignment: '%=';
PowerAssignment: '**=';

/*
    Keywords
*/
If: 'if';

/*Very important for newlines:

else
{
}
and
else if ...
{
}
and
else {
}

*/
Else: 'else';
True: 'true';
False: 'false';
Break: 'break';
Default: 'default';
Debugger: 'debugger';

As: 'as' Whitespace+;
For: 'for';
Ir: 'ir';
Case: 'case' Whitespace+;
Switch: 'switch';
While: 'while';
VariableDeclare: 'let' Whitespace+;
Import: 'import' Whitespace+;
EnumDeclare: 'enum' Whitespace+;
FunctionDeclare: 'function' Whitespace+;
Return: 'return' Whitespace+;
Throw: 'throw' Whitespace+;
Repeat: 'repeat';
Event: 'on' Whitespace+;
From: 'from' Whitespace+;
Namespace: 'namespace ' Whitespace+;
Type: 'number' | 'string' | 'boolean' | 'color' | 'any';
List: 'list';

/*
    Lexer rules
*/
Number: Digit+ ([.] Digit+)?; 
Identifier: [a-zA-Z_][a-zA-Z0-9_]*;
String: ('"' (~["\\\r\n] | '\\' ('"' | '\\'))* '"') | ('\'' (~['\\\r\n] | '\\' ('\'' | '\\'))* '\'');
Color: Hashtag HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit;

mode STRING_MODE;

EnterInterpolatedExpression: '${' -> pushMode(DEFAULT_MODE);
EscapeSequence: '\\' .;
Text: ~[\\`$]+;

CloseInterpolatedString: '`' -> popMode;
parser grammar ScratchScriptParser;

options {
    tokenVocab = ScratchScriptLexer;
}

program: topLevelStatement* EOF;
topLevelStatement: functionDeclarationStatement | enumDeclarationStatement | attributeStatement | eventStatement | importStatement | namespaceStatement;
line: ((statement Semicolon) | ifStatement | whileStatement | repeatStatement | forStatement | switchStatement | irBlockStatement | returnStatement | breakStatement | debuggerStatement | throwStatement | comment);
statement: assignmentStatement | listAssignmentStatement | functionCallStatement | memberFunctionCallStatement | variableDeclarationStatement | postIncrementStatement;

eventStatement: Event Identifier (LeftParen (expression (Comma expression)*?) RightParen)? block;
assignmentStatement: Identifier assignmentOperators expression;
listAssignmentStatement: Identifier LeftBracket expression RightBracket;
variableDeclarationStatement: VariableDeclare Identifier Assignment expression;
memberFunctionCallStatement: expression Dot functionCallStatement;
functionCallStatement: Identifier LeftParen (functionArgument (Comma functionArgument)*?)? RightParen; 
functionDeclarationStatement: attributeStatement*? FunctionDeclare Identifier LeftParen (typedIdentifier (Comma typedIdentifier)*?)? RightParen (Colon type)? block;

enumDeclarationStatement: EnumDeclare Identifier LeftBrace (enumEntry (Comma enumEntry)*?)? RightBrace;
enumEntry: Identifier (Assignment constant)?;

irBlockStatement: Ir LeftBrace (irStatement)*? RightBrace;
irStatement: Return? interpolatedString Semicolon;

ifStatement: If LeftParen expression RightParen block (Else elseIfStatement)?;
whileStatement: While LeftParen expression RightParen block;
forStatement: For LeftParen statement? Semicolon expression? Semicolon statement? RightParen block;
elseIfStatement: block | ifStatement;
postIncrementStatement: Identifier postIncrementOperators;
importStatement: Import ((LeftBrace Identifier (Comma Identifier)*? RightBrace) | importAll) From String Semicolon;
attributeStatement: At Identifier (LeftParen (constant (Comma constant)*?)? RightParen)?;
returnStatement: Return expression? Semicolon;
repeatStatement: Repeat LeftParen expression RightParen block;
throwStatement: Throw String Semicolon;
breakStatement: Break Semicolon;
namespaceStatement: Namespace String Semicolon;
switchStatement: Switch LeftParen expression RightParen switchBlock;
typedIdentifier: Identifier (Colon type)?;
functionArgument: (Identifier Colon)? expression;
debuggerStatement: Debugger Semicolon;

expression
    : constant #constantExpression
    | Identifier #identifierExpression
    | functionCallStatement #functionCallExpression
    | expression Dot functionCallStatement #memberFunctionCallExpression
    | expression Dot Identifier #memberPropertyAccessExpression
    | LeftBracket (expression (Comma expression)*?)? RightBracket #arrayInitializeExpression
    | LeftParen expression RightParen #parenthesizedExpression
    | Not expression #notExpression
    | expression LeftBracket expression RightBracket #arrayAccessExpression
    | addOperators expression #unaryAddExpression
    | expression bitwiseOperators expression #binaryBitwiseExpression
    | expression multiplyOperators expression #binaryMultiplyExpression
    | expression addOperators expression #binaryAddExpression
    | expression compareOperators expression #binaryCompareExpression
    | expression booleanOperators expression #binaryBooleanExpression
    | expression Ternary expression Colon expression #ternaryExpression
    | interpolatedString #interpolatedStringExpression
    ;

multiplyOperators: Multiply | Divide | Modulus | Power;

leftShift: first=Lesser second=Lesser {$first.index + 1 == $second.index}?;
rightShift: first=Greater second=Greater {$first.index + 1 == $second.index}?;

addOperators: Plus | Minus;
compareOperators: Equal | NotEqual | Greater | GreaterOrEqual | Lesser | LesserOrEqual;
booleanOperators: And | Or;
bitwiseOperators: BitwiseOr | BitwiseXor | BitwiseAnd | leftShift | rightShift;
assignmentOperators: Assignment | AdditionAsignment | SubtractionAssignment | MultiplicationAssignment | DivisionAssignment | ModulusAssignment | PowerAssignment;
postIncrementOperators: PostIncrement | PostDecrement;

case: (Case constant Colon block) | defaultCase;
block: LeftBrace line* RightBrace;
switchBlock: LeftBrace case* RightBrace;
defaultCase: Default Colon block;
importAll: Multiply (As Identifier)?;

constant: Number | String | boolean | Color;
comment: Comment;
boolean: True | False;
type
    : (Type | Identifier) #regularType
    | List Lesser type Greater #listType;

interpolatedString: OpenInterpolatedString interpolatedStringPart* CloseInterpolatedString;
interpolatedStringPart: EnterInterpolatedExpression expression RightBrace | Text | EscapeSequence ;
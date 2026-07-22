parser grammar ScratchScriptParser;

options {
    tokenVocab = ScratchScriptLexer;
}

program: topLevelStatement* EOF;
topLevelStatement: functionDeclarationStatement | enumDeclarationStatement | attributeStatement | eventStatement | importStatement | namespaceStatement;
line: ((statement Semicolon) | ifStatement | whileStatement | repeatStatement | forStatement | switchStatement | irBlockStatement | returnStatement | breakStatement | continueStatement | debuggerStatement | throwStatement | comment);
statement: assignmentStatement | listAssignmentStatement | functionCallStatement | memberFunctionCallStatement | variableDeclarationStatement | postIncrementStatement;

eventStatement: Event Identifier (LeftParen (expression (Comma expression)*?) RightParen)? block;
assignmentStatement: Identifier assignmentOperators expression;
listAssignmentStatement: Identifier LeftBracket expression RightBracket;
variableDeclarationStatement: VariableDeclare Identifier Assignment expression;
memberFunctionCallStatement: expression Dot functionCallStatement;
functionCallStatement: Identifier LeftParen (expression (Comma expression)*?)? RightParen; 
functionDeclarationStatement: attributeStatement*? FunctionDeclare Identifier LeftParen (functionDeclarationArgument (Comma functionDeclarationArgument)*?)? RightParen (Colon type)? block;

enumDeclarationStatement: EnumDeclare Identifier LeftBrace (enumEntry (Comma enumEntry)*?)? RightBrace;
enumEntry: Identifier (Assignment constant)?;

irBlockStatement: Ir LeftBrace (irStatement)*? RightBrace;
irStatement: Return? interpolatedString Semicolon;

ifStatement: If LeftParen expression RightParen lineOrBlock (Else lineOrBlock)?;
whileStatement: While LeftParen expression RightParen lineOrBlock;
forStatement: For LeftParen statement? Semicolon expression? Semicolon statement? RightParen lineOrBlock;
repeatStatement: Repeat LeftParen expression RightParen lineOrBlock;
lineOrBlock: line | block;

postIncrementStatement: Identifier postIncrementOperators;
importStatement: Import ((LeftBrace Identifier (Comma Identifier)*? RightBrace) | importAll) From String Semicolon;
attributeStatement: At Identifier (LeftParen (constant (Comma constant)*?)? RightParen)?;
returnStatement: Return expression? Semicolon;
throwStatement: Throw String Semicolon;
breakStatement: Break Semicolon;
continueStatement: Continue Semicolon;
namespaceStatement: Namespace String Semicolon;
switchStatement: Switch LeftParen expression RightParen switchBlock;
functionDeclarationArgument: attributeStatement*? Identifier (Colon type)?;
debuggerStatement: Debugger Semicolon;

expression
    // 1
    : constant                                                              #constantExpression
    | Identifier                                                            #identifierExpression
    | interpolatedString                                                    #interpolatedStringExpression
    | LeftParen expression RightParen                                       #parenthesizedExpression
    | LeftBracket (expression (Comma expression)*?)? RightBracket           #arrayInitializeExpression
    | functionCallStatement                                                 #functionCallExpression
    | LeftBrace (objectProperty (Comma objectProperty)*?)? RightBrace                                            #objectLiteralExpression
    // 2
    | expression Dot functionCallStatement                                  #memberFunctionCallExpression
    | expression Dot Identifier                                             #memberPropertyAccessExpression
    | expression LeftBracket expression RightBracket                        #arrayAccessExpression
    // 3
    | Not expression                                                        #notExpression
    | addOperators expression                                               #unaryAddExpression // e.g., +x, -x
    // 4
    | expression multiplyOperators expression                               #binaryMultiplyExpression
    // 5
    | expression addOperators expression                                    #binaryAddExpression
    // 6
    | expression bitwiseOperators expression                                #binaryBitwiseExpression
    // 7
    | expression compareOperators expression                                #binaryCompareExpression
    // 8
    | expression booleanOperators expression                                #binaryBooleanExpression
    // 9
    | expression Ternary expression Colon expression                        #ternaryExpression
    ;

objectProperty: propertyKey Colon expression;
propertyKey: Identifier | String;

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
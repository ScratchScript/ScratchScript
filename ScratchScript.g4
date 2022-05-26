grammar ScratchScript;
/*
Parser
*/

//operation: WHITESPACE* NUMBER WHITESPACE* '+' WHITESPACE* NUMBER;

/*
Lexer
*/

NUMBER: [0-9]+;
WHITESPACE: (' ' | '\t');
lexer grammar CustomLangLexer;


FUNC: 'func';

LPAREN: '(';
RPAREN: ')';
LBRACE: '{';
RBRACE: '}';
LBRACK: '[';
RBRACK: ']';
SEMI: ';';
COLON: ':';
COMMA: ',';
DOT: '.';
ARROW: '->';
ARROWEQ: '=>';
EQ: '=';
PLUS: '+';
MINUS: '-';
BANG: '!';
ASTERISK: '*';
SLASH: '/';
LT: '<';
LE: '<=';
GT: '>';
GE: '>=';
EQEQ: '==';
NE: '!=';
AMPERSAND: '&';
AND: '&&';
PIPE: '|';
OR: '||';

RANGE: '..';
IN: 'in';
FOR: 'for';
STEP: 'step';
LET: 'let';
RETURN: 'return';
IF: 'if';
ELSE: 'else';
OBJECT: 'object';
USES: 'uses';


PUBLIC: 'public';
PRIVATE: 'private';
PROTECTED: 'protected';
STATIC: 'static';
FINAL: 'final';
ABSTRACT: 'abstract';

// Types:
INT_TYPE: 'int';
FLOAT_TYPE: 'float';

// ID          : [a-zA-Z_][a-zA-Z_0-9]*;
INTEGER: [0-9]+;
FLOAT: ([0-9]+)? '.' [0-9]+;
STRING: '"' .*? '"';
WS: [ \t\r\n]+ -> skip;

RAW_STRING_LIT: '`' ~'`'* '`'; // -> mode(NLSEMI);
INTERPRETED_STRING_LIT:
	'"' (~["\\] | ESCAPED_VALUE)* '"'; // -> mode(NLSEMI);
IDENTIFIER: LETTER (LETTER | UNICODE_DIGIT)*; // -> mode(NLSEMI);
SINGLE_LINE_COMMENT: '//' ~[\r\n]* -> skip; // -> mode(NLSEMI);
MULTI_LINE_COMMENT: '/*' .*? '*/' -> skip; // -> mode(NLSEMI);



// Keywords Func : 'func';
//
//
//
// FUNC : 'func'; RETURN : 'return'; IF : 'if'; ELSE : 'else'; WHILE : 'while'; FOR : 'for'; BREAK :
// 'break'; CONTINUE : 'continue'; LET : 'let'; DOT : '.';
//
// L_CURLY : '{'; R_CURLY : '}' -> mode(NLSEMI);

fragment ESCAPED_VALUE:
	'\\' (
		'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
		| 'U' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
		| [abfnrtv\\'"]
		| OCTAL_DIGIT OCTAL_DIGIT OCTAL_DIGIT
		| 'x' HEX_DIGIT HEX_DIGIT
	);



fragment DECIMALS: [0-9] ('_'? [0-9])*;

fragment OCTAL_DIGIT: [0-7];

fragment HEX_DIGIT: [0-9a-fA-F];

fragment BIN_DIGIT: [01];

fragment EXPONENT: [eE] [+-]? DECIMALS;

fragment UNICODE_DIGIT: [\p{Nd}];

fragment LETTER: UNICODE_LETTER | '_';

fragment UNICODE_LETTER: [\p{L}];

//mode NLSEMI;

//WS_NLSEMI : [ \t]+ -> skip; // Ignore any comments that only span one line COMMENT_NLSEMI : '/*'
// ~[\r\n]*? '*/' -> skip; LINE_COMMENT_NLSEMI : '//' ~[\r\n]* -> skip; // Emit an EOS token for any
// newlines, semicolon, multiline comments or the EOF and //return to normal lexing EOS: ([\r\n]+ |
// ';' | '/*' .*? '*/' | EOF) -> mode(DEFAULT_MODE); // Did not find an EOS, so go back to normal
// lexing OTHER: -> mode(DEFAULT_MODE), channel(HIDDEN);

parser grammar CustomLangParser;



options {
	tokenVocab = CustomLangLexer;
//	superClass = CustomLangParserBase;
}

program     : (func | object)* EOF;
func        : FUNC IDENTIFIER LPAREN params? RPAREN type block;
params      : param (COMMA param)*;
param       : IDENTIFIER type;
block       : LBRACE (statement*) RBRACE;

statement   
    : letStatement
    | loopStatement
    | callExpression
    | ifStatement
    | returnStatement
    ;

letStatement     : LET IDENTIFIER COLON type (EQ primaryExpression)? SEMI;
ifStatement      : IF LPAREN? primaryExpressionNonParen RPAREN? block elseStatement?;
elseStatement    : ELSE ifStatement | ELSE block | block;
returnStatement  : RETURN primaryExpression SEMI;

loopStatement
    : FOR loopHead? block
    ;
    
loopHead
    : loopRange
    | IDENTIFIER IN loopRange
    ;
    
loopRange
    : rangeLower=primaryExpression RANGE rangeUpper=primaryExpression (STEP step=primaryExpression)?
    ;

object
    : OBJECT IDENTIFIER LBRACE(usesStatement | objectField SEMI?)* RBRACE ARROW LBRACE (objectBody)* RBRACE;

objectHead:
      (usesStatement)*
//    | (objectField SEMI)*
    ;

usesStatement: USES useList SEMI?;
useList : singleUse | useBlock;
singleUse: IDENTIFIER;
useBlock: LBRACE useSequence RBRACE;
useSequence: IDENTIFIER (COMMA IDENTIFIER?)*;


objectBody
    : modifier? IDENTIFIER parameters type? block
    ;

objectField: modifier? IDENTIFIER COLON? type (EQ primaryExpression)?;

modifier: PUBLIC | PRIVATE | PROTECTED | STATIC | FINAL | ABSTRACT;

parameters: LPAREN (parameterDecl (COMMA parameterDecl)* COMMA?)? RPAREN;

parameterDecl: IDENTIFIER COLON? IDENTIFIER;

type : INT_TYPE | FLOAT_TYPE;


args : primaryExpression (COMMA primaryExpression)*;
callExpression : IDENTIFIER LPAREN (args)? RPAREN SEMI?;

value 
    : callExpression
    | STRING
    | FLOAT
    | INTEGER
    | id
    ;

// Primary expressions
primaryExpressionNonParen
    : value
    | expression                // Parenthesized expression
    ;
    
// Primary expressions
primaryExpression
    : value
    | LPAREN expression RPAREN       // Parenthesized expression
    ;

// Multiplicative expressions
multiplicativeExpression
    : expr=primaryExpression                                              // Primary expressions
    | lhs=multiplicativeExpression op=ASTERISK rhs=primaryExpression // Multiplication
    | lhs=multiplicativeExpression op=SLASH    rhs=primaryExpression // Division
    ;

// Additive expressions
additiveExpression
    : expr=multiplicativeExpression                                           // Multiplicative expressions
    | lhs=additiveExpression op=PLUS  rhs=multiplicativeExpression       // Addition
    | lhs=additiveExpression op=MINUS rhs=multiplicativeExpression       // Subtraction
    ;

// Relational expressions
relationalExpression
    : expr=additiveExpression                                         // Additive expressions
    | lhs=relationalExpression op=LT   rhs=additiveExpression    // Less than
    | lhs=relationalExpression op=GT   rhs=additiveExpression    // Greater than
    | lhs=relationalExpression op=LE   rhs=additiveExpression    // Less than or equal to
    | lhs=relationalExpression op=GE   rhs=additiveExpression    // Greater than or equal to
    | lhs=relationalExpression op=EQEQ rhs=additiveExpression    // Equal to
    | lhs=relationalExpression op=NE   rhs=additiveExpression    // Not equal to
    ;

// Logical AND expressions
logicalAndExpression
    : expr=relationalExpression                                      // Relational expressions
    | lhs=logicalAndExpression op=AND rhs=relationalExpression  // Logical AND
    ;

// Logical OR expressions
logicalOrExpression
    : expr=logicalAndExpression                                      // Logical AND expressions
    | lhs=logicalOrExpression op=OR rhs=logicalAndExpression    // Logical OR
    ;

// Assignment expressions
expression
    : expr=logicalOrExpression                  // Logical OR expressions
    | lhs=id op=EQ rhs=expression  // Assignment
    ;


id : IDENTIFIER;

/*

//
//
//program : line* EOF;
//
//line : (functionDecl | varDecl);
//
//block: L_CURLY statementList? R_CURLY;
//
//declaration: varDecl;
//
//
//varDecl: VAR (varSpec | L_PAREN (varSpec eos)* R_PAREN);
//varSpec:
//	identifierList (
//		type_ (ASSIGN expressionList)?
//		| ASSIGN expressionList
//	);
//
//statementList: ((SEMI? | EOS? ) statement eos)+;
//statement : declaration;
//
//functionDecl: FUNC IDENTIFIER signature block?;
//functionType: FUNC signature;
//signature:
//	parameters result
//	| parameters;
//result: parameters | type_;
//
//parameters:
//	L_PAREN (parameterDecl (COMMA parameterDecl)* COMMA?)? R_PAREN;
//parameterDecl: identifierList? ELLIPSIS? type_;
//
//type_ : IDENTIFIER;
//typeName: qualifiedIdent | IDENTIFIER;
//
//identifierList: IDENTIFIER (COMMA IDENTIFIER)*;
//qualifiedIdent: IDENTIFIER DOT IDENTIFIER;
//
//expressionList: expression (COMMA expression)*;
//
//expression: primaryExpr;
//primaryExpr: operand;
//
//operand : basicLit;
//
//string_: RAW_STRING_LIT | INTERPRETED_STRING_LIT;
//
//basicLit:
//	NIL_LIT
//	| integer
//	| string_
//	| FLOAT_LIT;
//
//integer:
//	DECIMAL_LIT
//	| BINARY_LIT
//	| OCTAL_LIT
//	| HEX_LIT
//	| IMAGINARY_LIT
//	| RUNE_LIT;
//
//
//eos:
//	SEMI
//	| EOF
//	| EOS
////	| {this.closingBracket()}?
//	;
*/

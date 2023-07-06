using System;
using Antlr4.Runtime;
using CustomLanguage.Grammar;

namespace CustomLanguage.Compiler;

public class ParsingResult
{

    public AntlrInputStream                AntlrInputStream  = null!;
    public CustomLangLexer                 Lexer             = null!;
    public CustomLangParser                Parser            = null!;
    public CommonTokenStream               CommonTokenStream = null!;
    public CustomLangParser.ProgramContext Program           = null!;
    public ParserRuleContext               ParseTree         = null!;

    public Exception? LexerParserUnhandledException = null;

    public ParserErrorListener ParseErrorListener = null!;

    /// <summary>
    /// true if at least one error or an unhandled exception after tried parsing
    /// </summary>
    public bool HasErrors => ParseErrorListener.HasErrors ||LexerParserUnhandledException != null;

    public void WriteError()
    {
        if (LexerParserUnhandledException != null) {
            Console.WriteLine(LexerParserUnhandledException);
            return;
        }
        
        foreach (var error in ParseErrorListener.Errors) {
            Console.WriteLine(error);
        }
    }
}
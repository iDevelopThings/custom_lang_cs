using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using CustomLanguage.Grammar;

namespace CustomLanguage.Compiler;

public class LexerParser
{
    static LexerParser _Instance = null!;

    private List<CustomLangParserBaseListener> _listeners = new();

    public static LexerParser Instance
    {
        get { return _Instance ??= new LexerParser(); }
    }

    public ParsingResult Parse(string text)
    {
        var result = new ParsingResult();
        // try {
            var antlrInputStream = new AntlrInputStream(text);
            result.AntlrInputStream = antlrInputStream;

            var lexer = new CustomLangLexer(antlrInputStream);
            result.Lexer = lexer;

            var commonTokenStream = new CommonTokenStream(lexer);
            result.CommonTokenStream = commonTokenStream;

            var parser = new CustomLangParser(commonTokenStream);
            result.Parser = parser;

            parser.RemoveErrorListeners();
            var parseErrorListener = new ParserErrorListener(true);
            parser.AddErrorListener(parseErrorListener);
            result.ParseErrorListener = parseErrorListener;

            result.ParseTree = parser.program();

            if (parseErrorListener.HasErrors) {
                return result;
            }

            var walker = new Antlr4.Runtime.Tree.ParseTreeWalker();
            foreach (var listener in _listeners) {
                walker.Walk(listener, result.ParseTree);
            }
        // }
        // catch (Exception ex) {
            // result.LexerParserUnhandledException = ex;
        // }

        return result;
    }

    public void AddListeners(CustomLangParserBaseListener[] listeners)
    {
        this._listeners.AddRange(listeners);
    }
}
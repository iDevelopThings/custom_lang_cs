using System;
using System.Collections.Generic;
using System.IO;
using CustomLanguage.Grammar;
using CustomLanguage.Passes;
using LLVMSharp.Interop;
using static CustomLanguage.Grammar.CustomLangParser;

namespace CustomLanguage.Compiler;

public class Source
{
    public string          FilePath = null!;
    public string          Input    = null!;
    public ProgramContext? ast;
    public ParsingResult   parseResult;
}

public class Compiler
{
    private        DateTime     _initializedAt;
    private        DateTime     _finishedAt;
    private static Compiler?    _instance;
    public static  Compiler     Instance => _instance ??= new Compiler();
    private        List<Source> _sources = new();

    public void Initialize()
    {
        _initializedAt = DateTime.Now;
        LexerParser.Instance.AddListeners(new CustomLangParserBaseListener[] {
            new Passes.FirstPassListener(),
            // new Passes.CodeGenListener(),
        });
        
        
    }

    public void AddSource(string filePath)
    {
        _sources.Add(new Source {
            FilePath = filePath,
            Input    = File.ReadAllText(Path.Combine(ProjectSourcePath.Value, "testing", "inputs", filePath))
        });
    }

    private bool ParseSources()
    {
        var hasErrors = false;

        foreach (var source in _sources) {
            var result = LexerParser.Instance.Parse(source.Input);
            source.parseResult = result;

            if (result.HasErrors) {
                hasErrors = true;
                Console.WriteLine($"Failed to parse {source.FilePath}");
                result.WriteError();
                continue;
            }

            var codeGenVisitor = new CodeGenVisitor();
            codeGenVisitor.Visit(result.ParseTree);

            source.ast = result.ParseTree as ProgramContext;
        }

        return !hasErrors;
    }

    public bool Compile()
    {
        if (!Ctx.Initialize()) {
            Console.WriteLine("Failed to initialize LLVM.");
            return false;
        }

        if (!ParseSources()) {
            Console.WriteLine("Failed to parse sources.");
            return false;
        }


        _finishedAt = DateTime.Now;

        return true;
    }

    public void FinalizeCompilation()
    {
        Console.WriteLine($"Took: {(_finishedAt - _initializedAt).TotalMilliseconds}ms");

        Console.WriteLine(new string('-', 50));
        Ctx.Module.Dump();
        Console.WriteLine(new string('-', 50));

        if (!Ctx.Module.TryVerify(LLVMVerifierFailureAction.LLVMPrintMessageAction, out var error)) {
            Console.WriteLine(error);
            return;
        }

        var output = Ctx.Engine.RunFunction(Ctx.Module.GetNamedFunction("main"), Array.Empty<LLVMGenericValueRef>());

        Console.WriteLine(new string('-', 50));
    }
}
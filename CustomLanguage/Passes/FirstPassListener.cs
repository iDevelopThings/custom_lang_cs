using System;
using CustomLanguage.Compiler;
using CustomLanguage.Core;
using CustomLanguage.Grammar;
using CustomLanguage.Types;
using LLVMSharp.Interop;

namespace CustomLanguage.Passes;

public class FirstPassListener : CustomLangParserBaseListener
{
    public override void EnterFunc(CustomLangParser.FuncContext context)
    {
        Console.WriteLine("[FirstPassListener] - EnterFunc - " + context.IDENTIFIER());

        // var scope = Scope.Push();
        // Scope.LinkToNode(context);
        Scope.DefineFunction(context.IDENTIFIER().Symbol.Text, new Function(context));


        // var funcType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new LLVMTypeRef[] { }, false);
        // var func     = Ctx.Module.AddFunction("main", funcType);
        // var block    = func.AppendBasicBlock("entry");
        // Ctx.Builder.PositionAtEnd(block);
        // Ctx.Builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false));
    }

    public override void ExitFunc(CustomLangParser.FuncContext context)
    {
        Console.WriteLine("[FirstPassListener] - ExitFunc - " + context.IDENTIFIER());
        // Scope.Pop();

    }
}
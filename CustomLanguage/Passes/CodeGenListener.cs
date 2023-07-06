using System;
using CustomLanguage.Compiler;
using CustomLanguage.Core;
using CustomLanguage.Grammar;
using CustomLanguage.Types;
using LLVMSharp;
using LLVMSharp.Interop;

namespace CustomLanguage.Passes;

public class CodeGenListener : CustomLangParserBaseListener
{
    /*public override void EnterFunc(CustomLangParser.FuncContext context)
    {
        Console.WriteLine("[CodeGenVisitor] - EnterFunc - " + context.IDENTIFIER());

        var scope               = Scope.Push();
        var functionDeclaration = scope.LookupFunction(context.IDENTIFIER().GetText());

        var funcType = LLVMTypeRef.CreateFunction(
            Scope.LookupType(functionDeclaration.ReturnType)!.LLVMType(),
            functionDeclaration.Parameters.Select(x => Scope.LookupType(x.Type)!.LLVMType()).ToArray()
        );

        functionDeclaration.Type = funcType;
        var func = Ctx.Module.AddFunction(functionDeclaration.Name, funcType);

        Scope.Global.DefineFunctionDefinition(funcType, func);

        for (int i = 0; i < func.Params.Length; i++) {
            scope.Vars.Add(functionDeclaration.Parameters[i].Name, func.Params[i]);
        }


        var block = func.AppendBasicBlock("entry");
        Ctx.Builder.PositionAtEnd(block);
        // Ctx.Builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false));
    }

    public override void ExitFunc(CustomLangParser.FuncContext context)
    {
        Console.WriteLine("[CodeGenVisitor] - ExitFunc - " + context.IDENTIFIER());
        Scope.Pop();
    }

    /// <inheritdoc />
    public override void EnterStatement(CustomLangParser.StatementContext context)
    {
        Console.WriteLine("[CodeGenVisitor] - EnterStatement - " + context.GetText());
        Ctx.Builder.BuildAlloca(LLVMTypeRef.Int1, "test");
    }

    public override void EnterLetStatement(CustomLangParser.LetStatementContext context)
    {
        var alloc = Ctx.Builder.BuildAlloca(
            Scope.LookupType(context.type().GetText())!.LLVMType(),
            context.IDENTIFIER().GetText()
        );

        Scope.Current.Allocations.Add(context.IDENTIFIER().GetText(), alloc);

        if (context.EQ() != null) {
            // context.result = 1;
            // LLVMValueRef.CreateConstInt(

            // )
        }
    }

    public override void EnterBlock(CustomLangParser.BlockContext context)
    {
    }*/
}
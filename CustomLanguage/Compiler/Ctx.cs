using System;
using CustomLanguage.Core;
using CustomLanguage.Types;
using LLVMSharp;
using LLVMSharp.Interop;

namespace CustomLanguage.Compiler;

public class Ctx
{

    public static LLVMModuleRef  Module  { get; set; }
    public static LLVMContextRef Context { get; set; }
    public static LLVMBuilderRef Builder { get; set; }

    public static LLVMExecutionEngineRef Engine          { get; set; }
    public static LLVMValueRef           CurrentFunction { get; set; }
    public static int                    StringCounter   { get; set; }


    public static bool Initialize()
    {
        Module  = LLVMModuleRef.CreateWithName("main");
        Context = Module.Context;
        Builder = Context.CreateBuilder();

        // Context = LLVMContextRef.Create();
        // Module  = Context.CreateModuleWithName("CustomLang");
        // Builder = LLVMBuilderRef.Create(Module.Context);

        LLVM.LinkInMCJIT();
        LLVM.InitializeX86TargetMC();
        LLVM.InitializeX86Target();
        LLVM.InitializeX86TargetInfo();
        LLVM.InitializeX86AsmParser();
        LLVM.InitializeX86AsmPrinter();

        Engine = Module.CreateExecutionEngine();

        var printfFuncType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] {LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)}, true);
        var printfFunc     = Module.AddFunction("printf", printfFuncType);
        Scope.DefineFunction("printf", new Types.Function() {
            Name = "printf",
            Parameters = {
                new() {
                    Name = "format",
                    Type = "string"
                },
            },
            Type       = printfFuncType,
            ReturnType = "int",
            IsVariadic = true,
        });

        return true;
    }
}
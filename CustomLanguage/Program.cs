using LLVMSharp;
using LLVMSharp.Interop;
using CustomLanguage.Compiler;

var filePath = "main_func.lang";
if (args.Length > 0) {
    filePath = args[0];
}

var compiler = Compiler.Instance;

compiler.Initialize();
compiler.AddSource(filePath);
if (!compiler.Compile()) {
    return;
}

compiler.FinalizeCompilation();

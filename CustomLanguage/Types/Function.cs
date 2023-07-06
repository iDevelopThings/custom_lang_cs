using CustomLanguage.Grammar;
using LLVMSharp.Interop;

namespace CustomLanguage.Types;

public class FunctionParameter
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
}

public class Function : Type
{
    public string                  Name       { get; set; }
    public string                  ReturnType { get; set; }
    public List<FunctionParameter> Parameters { get; set; } = new();
    public LLVMTypeRef             Type       { get; set; }
    public bool                    IsVariadic { get; set; }

    public Function()
    {
    }

    public Function(CustomLangParser.FuncContext context)
    {
        Name       = context.IDENTIFIER().GetText();
        ReturnType = context.type().GetText();
        var parameters = context.@params();
        if (parameters == null) {
            return;
        }

        for (int i = 0; i < parameters.ChildCount; i++) {
            var child = context.@params().GetChild(i);
            if (child is CustomLangParser.ParamContext paramContext) {
                Parameters.Add(new FunctionParameter {
                    Name = paramContext.IDENTIFIER().Symbol.Text,
                    Type = paramContext.type().GetText(),
                });
            }
        }
    }

    /// <inheritdoc />
    public LLVMTypeRef LLVMType()
    {
        return Type;
    }
}
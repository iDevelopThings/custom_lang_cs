using Antlr4.Runtime;
using CustomLanguage.Grammar;
using CustomLanguage.Types;
using Type = CustomLanguage.Types.Type;

namespace CustomLanguage.Core;

public class Scope
{
    public static Dictionary<RuleContext, SymbolTable> LinkedScopes { get; set; } = new();

    public static SymbolTable Current { get; set; } = new();

    public static SymbolTable Global { get; set; } = Current;

    public static SymbolTable Push()
    {
        Current = new SymbolTable {Parent = Current};

        Console.WriteLine("Enter scope");

        return Current;
    }

    public static void Pop()
    {
        Console.WriteLine("Exit scope");
        if (Current.Parent is null)
            return;

        Current = Current.Parent!;
    }

    public static void DefineSymbol(string name, string type) =>
        Current.DefineSymbol(name, type);

    public static void DefineFunction(string name, Function function)
        => Current.DefineFunction(name, function);

    public static Type? LookupType(string name) => Current.LookupType(name);

    public static SymbolTable? GetLinked(RuleContext context)
    {
        if (LinkedScopes.TryGetValue(context, out var scope)) {
            return scope;
        }

        return null;
    }
}
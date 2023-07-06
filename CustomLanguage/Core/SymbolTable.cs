using Antlr4.Runtime;
using CustomLanguage.Grammar;
using CustomLanguage.Types;
using LLVMSharp;
using LLVMSharp.Interop;
using Function = CustomLanguage.Types.Function;
using Type = CustomLanguage.Types.Type;

namespace CustomLanguage.Core;

public class Symbol
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}

public struct TypedValue
{
    public LLVMTypeRef  Type;
    public LLVMValueRef Value;
}

public class SymbolTable : IDisposable
{
    public Dictionary<string, Type>         Types               { get; set; } = new();
    public Dictionary<string, Symbol>       Symbols             { get; set; } = new();
    public Dictionary<string, Function>     Functions           { get; set; } = new();
    public Dictionary<string, TypedValue>   FunctionDefinitions { get; set; } = new();
    public SymbolTable?                     Parent              { get; set; }
    public Dictionary<string, LLVMValueRef> Vars                { get; set; } = new();


    public SymbolTable()
    {
        if (Scope.Current?.Parent != null) {
            return;
        }

        foreach (var (key, value) in Native.Types) {
            Types.Add(key, value);
        }
    }

    public void DefineSymbol(string name, string type)
    {
        Symbols.Add(name, new Symbol {Name = name, Type = type});
    }

    public LLVMValueRef DefineVar(string name, LLVMValueRef value)
    {
        Vars.Add(name, value);
        return value;
    }

    public void DefineFunction(string name, Function function)
    {
        Functions.Add(name, function);
    }

    public void DefineFunctionDefinition(LLVMTypeRef type, LLVMValueRef value)
    {
        FunctionDefinitions.Add(value.Name, new TypedValue {Type = type, Value = value});
    }

    public void Dispose()
    {
        if (this.Parent is null)
            return;

        Scope.Pop();
    }

    public void LinkToNode(RuleContext context)
    {
        Scope.LinkedScopes.Add(context, this);
    }

    public Type? LookupType(string name)
    {
        if (Types.TryGetValue(name, out var type)) {
            return type;
        }

        if (Parent != null) {
            return Parent.LookupType(name);
        }

        return null;
    }

    public Function LookupFunction(string name)
    {
        if (Functions.TryGetValue(name, out var function)) {
            return function;
        }

        if (Parent != null) {
            return Parent.LookupFunction(name);
        }

        return null!;
    }

    public LLVMValueRef? LookupVariable(string name)
    {
        if (Vars.TryGetValue(name, out var value)) {
            return value;
        }

        if (Parent != null) {
            return Parent.LookupVariable(name);
        }

        return null;
    }
}
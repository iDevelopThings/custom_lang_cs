using LLVMSharp.Interop;

namespace CustomLanguage.Types;

public class Native
{

    public static readonly Int Int   = new("int", 32);
    public static readonly Int Int8  = new("i8", 8);
    public static readonly Int Int16 = new("i16", 16);
    public static readonly Int Int32 = new("i32", 32);
    public static readonly Int Int64 = new("i64", 64);
    
    public static readonly Float Float = new();

    public static Dictionary<string, Type> Types = new() {
        {"int", Int},
        {"i8", Int8},
        {"i16", Int16},
        {"i32", Int32},
        {"i64", Int64},
        {"float", Float}
    };

}

public class Int : Type
{
    public string Name    { get; }
    public uint   BitSize { get; }

    public Int(string name, uint bitSize)
    {
        BitSize = bitSize;
        Name    = name;
    }

    public LLVMTypeRef LLVMType()
    {
        return LLVMTypeRef.CreateInt(BitSize);
    }

}

public class Float : Type
{
    public string Name { get; } = "float";

    public LLVMTypeRef LLVMType() =>  LLVMTypeRef.Float;
}
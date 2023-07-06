using LLVMSharp.Interop;

namespace CustomLanguage.Types;

public interface Type
{
    public string Name { get; }

    public LLVMTypeRef LLVMType();
}
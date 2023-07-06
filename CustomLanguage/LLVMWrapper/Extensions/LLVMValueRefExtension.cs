using LLVMSharp.Interop;

namespace CustomLanguage.LLVMWrapper.Extensions;

public static class LLVMValueRefExtension
{

    public static LLVMTypeRef GetAllocatedType(this LLVMValueRef value)
    {
        if (value.Kind == LLVMValueKind.LLVMArgumentValueKind) {
            return value.TypeOf;
        } else if (value.Kind == LLVMValueKind.LLVMInstructionValueKind) {
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind) {
                return value.TypeOf.ElementType;
            }

            return value.TypeOf;
        }

        return value.TypeOf;
    }

}
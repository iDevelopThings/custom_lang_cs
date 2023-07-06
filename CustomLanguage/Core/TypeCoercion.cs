using CustomLanguage.Compiler;
using CustomLanguage.LLVMWrapper.Extensions;
using LLVMSharp.Interop;

namespace CustomLanguage.Core;

public class TypeCoercion
{

    public static (LLVMValueRef, LLVMValueRef) Coerce(LLVMValueRef value1, LLVMValueRef value2)
    {
        var type1 = value1.TypeOf;
        var type2 = value2.TypeOf;

        if (type1 == type2) {
            return (value1, value2); // Types are the same, no coercion needed
        }

        if (type1.Kind == LLVMTypeKind.LLVMFloatTypeKind && type2.Kind == LLVMTypeKind.LLVMIntegerTypeKind) {
            // Convert int to float
            return (value1, Ctx.Builder.BuildSIToFP(value2, type1));
        }

        if (type1.Kind == LLVMTypeKind.LLVMIntegerTypeKind && type2.Kind == LLVMTypeKind.LLVMFloatTypeKind) {
            // Convert int to float
            return (Ctx.Builder.BuildSIToFP(value1, type2), value2);
        }

        if (type1.Kind == LLVMTypeKind.LLVMIntegerTypeKind && type2.Kind == LLVMTypeKind.LLVMIntegerTypeKind) {
            // Both are integers, but their bit sizes may be different
            if (type1.IntWidth() < type2.IntWidth()) {
                // Extend type1 to match the width of type2
                var extended = Ctx.Builder.BuildSExt(value1, type2);
                return (extended, value2);
            } else if (type1.IntWidth() > type2.IntWidth()) {
                // Extend type2 to match the width of type1
                var extended = Ctx.Builder.BuildSExt(value2, type1);
                return (value1, extended);
            }
        }

        throw new Exception($"Cannot coerce {type1.Kind} to {type2.Kind}");
    }



}
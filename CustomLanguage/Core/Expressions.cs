using Antlr4.Runtime;
using CustomLanguage.Compiler;
using CustomLanguage.Grammar;
using CustomLanguage.Passes;
using LLVMSharp.Interop;

namespace CustomLanguage.Core;

public enum Operators
{
    LT     = CustomLangLexer.LT,
    GT     = CustomLangLexer.GT,
    LE     = CustomLangLexer.LE,
    GE     = CustomLangLexer.GE,
    ASSIGN = CustomLangLexer.EQ,
    EQ     = CustomLangLexer.EQEQ,
    NE     = CustomLangLexer.NE,
    PLUS   = CustomLangLexer.PLUS,
    MINUS  = CustomLangLexer.MINUS,
    MUL    = CustomLangLexer.ASTERISK,
    DIV    = CustomLangLexer.SLASH,
    AND    = CustomLangLexer.AND,
    OR     = CustomLangLexer.OR,
}

public static class OperatorsExtensions
{
    public static Operators ToOperator(this int tokenType)
    {
        return (Operators) tokenType;
    }

    public static Operators FromIToken(IToken token)
    {
        return token.Type.ToOperator();
    }

    public static LLVMIntPredicate ToLLVMIntPredicate(Operators op)
    {
        return op switch {
            Operators.LT => LLVMIntPredicate.LLVMIntSLT,
            Operators.GT => LLVMIntPredicate.LLVMIntSGT,
            Operators.LE => LLVMIntPredicate.LLVMIntSLE,
            Operators.GE => LLVMIntPredicate.LLVMIntSGE,
            Operators.EQ => LLVMIntPredicate.LLVMIntEQ,
            Operators.NE => LLVMIntPredicate.LLVMIntNE,
            _            => throw new Exception($"Operator {op} is not an integer operator")
        };
    }

    public static LLVMRealPredicate ToLLVMRealPredicate(Operators op)
    {
        return op switch {
            Operators.LT => LLVMRealPredicate.LLVMRealOLT,
            Operators.GT => LLVMRealPredicate.LLVMRealOGT,
            Operators.LE => LLVMRealPredicate.LLVMRealOLE,
            Operators.GE => LLVMRealPredicate.LLVMRealOGE,
            Operators.EQ => LLVMRealPredicate.LLVMRealOEQ,
            Operators.NE => LLVMRealPredicate.LLVMRealONE,
            _            => throw new Exception($"Operator {op} is not a float operator")
        };
    }

    public static LLVMValueRef BuildIntComparison(this Operators op, LLVMValueRef left, LLVMValueRef right)
    {
        return Ctx.Builder.BuildICmp(ToLLVMIntPredicate(op), left, right);
    }

    public static LLVMValueRef BuildFloatComparison(this Operators op, LLVMValueRef left, LLVMValueRef right)
    {
        return Ctx.Builder.BuildFCmp(ToLLVMRealPredicate(op), left, right);
    }


}

public class Expressions
{

    public static Dictionary<Operators, Func<LLVMValueRef, LLVMValueRef, LLVMValueRef>> IntFunctions = new() {
        {Operators.LT, (left,    right) => Ctx.Builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, left, right)},
        {Operators.GT, (left,    right) => Ctx.Builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, left, right)},
        {Operators.LE, (left,    right) => Ctx.Builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, left, right)},
        {Operators.GE, (left,    right) => Ctx.Builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, left, right)},
        {Operators.EQ, (left,    right) => Ctx.Builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right)},
        {Operators.NE, (left,    right) => Ctx.Builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right)},
        {Operators.PLUS, (left,  right) => Ctx.Builder.BuildAdd(left, right)},
        {Operators.MINUS, (left, right) => Ctx.Builder.BuildSub(left, right)},
        {Operators.MUL, (left,   right) => Ctx.Builder.BuildMul(left, right)},
        {Operators.DIV, (left,   right) => Ctx.Builder.BuildSDiv(left, right)},
    };

    public static Dictionary<Operators, Func<LLVMValueRef, LLVMValueRef, LLVMValueRef>> FloatFunctions = new() {
        {Operators.LT, (left,    right) => Ctx.Builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, left, right)},
        {Operators.GT, (left,    right) => Ctx.Builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, left, right)},
        {Operators.LE, (left,    right) => Ctx.Builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, left, right)},
        {Operators.GE, (left,    right) => Ctx.Builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, left, right)},
        {Operators.EQ, (left,    right) => Ctx.Builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, left, right)},
        {Operators.NE, (left,    right) => Ctx.Builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, left, right)},
        {Operators.PLUS, (left,  right) => Ctx.Builder.BuildFAdd(left, right)},
        {Operators.MINUS, (left, right) => Ctx.Builder.BuildFSub(left, right)},
        {Operators.MUL, (left,   right) => Ctx.Builder.BuildFMul(left, right)},
        {Operators.DIV, (left,   right) => Ctx.Builder.BuildFDiv(left, right)},
    };

    public static CodeGenResult GenerateExpression(Operators op, LLVMValueRef left, LLVMValueRef right)
    {
        var (lhs, rhs) = TypeCoercion.Coerce(left, right);

        var lhsType = lhs.TypeOf;
        var rhsType = rhs.TypeOf;

        if (lhsType != rhsType) {
            throw new Exception("Cannot create binary expression with different types");
        }

        var actions = lhsType.Kind switch {
            LLVMTypeKind.LLVMIntegerTypeKind => IntFunctions,
            LLVMTypeKind.LLVMFloatTypeKind   => FloatFunctions,
            _                                => throw new Exception("Unknown type " + lhsType.Kind)
        };

        if (actions.TryGetValue(op, out var action)) {
            return new CodeGenResult().Add(action(lhs, rhs));
        }

        throw new Exception("Unknown operator " + op.ToString());
    }


}
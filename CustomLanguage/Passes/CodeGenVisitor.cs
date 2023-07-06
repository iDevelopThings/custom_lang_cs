using Antlr4.Runtime;
using CustomLanguage.Compiler;
using CustomLanguage.Core;
using CustomLanguage.Grammar;
using LLVMSharp;
using LLVMSharp.Interop;

namespace CustomLanguage.Passes;

public struct CodeGenResult
{
    List<LLVMValueRef> Values { get; set; } = new();

    public CodeGenResult()
    {
        Values = new List<LLVMValueRef>();
    }

    public CodeGenResult Add(LLVMValueRef value)
    {
        Values.Add(value);

        return this;
    }

    public CodeGenResult Add(CodeGenResult result)
    {
        Values.AddRange(result.Values);

        return this;
    }

    public LLVMValueRef First()
    {
        if (Values == null || Values.Count == 0) {
            return null;
        }

        return Values.First();
    }

    public bool Empty()
    {
        if (Values == null || Values.Count == 0) {
            return true;
        }

        return Values.Count == 0;
    }

    public static implicit operator LLVMValueRef(CodeGenResult result)
    {
        return result.First();
    }

    public static CodeGenResult From(Func<LLVMValueRef> func)
    {
        return new CodeGenResult().Add(func());
    }
}

public class CodeGenVisitor : CustomLangParserBaseVisitor<CodeGenResult>
{

    public struct ExpressionNode
    {
        public ParserRuleContext? lhs { get; set; }
        public IToken?            op  { get; set; }
        public ParserRuleContext? rhs { get; set; }

        public ExpressionNode(
            ParserRuleContext  defaultNode,
            ParserRuleContext? lhs,
            IToken?            op,
            ParserRuleContext? rhs
        )
        {
            if (op == null) {
                this.lhs = defaultNode;
                return;
            }

            this.lhs = lhs;
            this.op  = op;
            this.rhs = rhs;
        }
    }

    public override CodeGenResult VisitFunc(CustomLangParser.FuncContext context)
    {
        Console.WriteLine("[CodeGenVisitor] - EnterFunc - " + context.IDENTIFIER());

        var scope               = Scope.Push();
        var functionDeclaration = scope.LookupFunction(context.IDENTIFIER().GetText());

        var funcType = LLVMTypeRef.CreateFunction(
            Scope.LookupType(functionDeclaration.ReturnType)!.LLVMType(),
            functionDeclaration.Parameters.Select(x => Scope.LookupType(x.Type)!.LLVMType()).ToArray()
        );

        functionDeclaration.Type = funcType;
        var func = Ctx.Module.AddFunction(functionDeclaration.Name, funcType).IsAFunction;

        Scope.Global.DefineFunctionDefinition(funcType, func);

        Ctx.CurrentFunction = func;

        for (int i = 0; i < func.Params.Length; i++) {
            scope.Vars.Add(functionDeclaration.Parameters[i].Name, func.Params[i]);
        }


        var block = func.AppendBasicBlock("entry");
        Ctx.Builder.PositionAtEnd(block);

        if (context.block() != null) {
            VisitBlock(context.block());
        }

        Console.WriteLine("[CodeGenVisitor] - ExitFunc - " + context.IDENTIFIER());
        Scope.Pop();
        Ctx.CurrentFunction = null;

        return new CodeGenResult().Add(
            func
        );
    }

    // public override CodeGenResult VisitStatement(CustomLangParser.StatementContext context)
    // {
    //     Console.WriteLine("[CodeGenVisitor] - EnterStatement - " + context.GetText());
    //     var alloc = Ctx.Builder.BuildAlloca(LLVMTypeRef.Int1, "test");
    // }

    public override CodeGenResult VisitLetStatement(CustomLangParser.LetStatementContext context)
    {
        var alloc = Ctx.Builder.BuildAlloca(
            Scope.LookupType(context.type().GetText())!.LLVMType(),
            context.IDENTIFIER().GetText()
        );

        Scope.Current.DefineVar(context.IDENTIFIER().GetText(), alloc);

        if (context.EQ() != null && context.primaryExpression() != null) {
            var result = VisitPrimaryExpression(context.primaryExpression());
            if (!result.Empty()) {
                Ctx.Builder.BuildStore(result, alloc);
            }
        }

        return new CodeGenResult().Add(
            alloc
        );
    }

    public override CodeGenResult VisitReturnStatement(CustomLangParser.ReturnStatementContext context)
    {
        var result = VisitPrimaryExpression(context.primaryExpression());
        if (!result.Empty()) {
            Ctx.Builder.BuildRet(result);
        }

        return result;
    }

    /// <inheritdoc />
    public override CodeGenResult VisitIfStatement(CustomLangParser.IfStatementContext context)
    {
        var result = new CodeGenResult();

        var condition = VisitPrimaryExpressionNonParen(context.primaryExpressionNonParen());

        var thenBlock  = Ctx.CurrentFunction.AppendBasicBlock("then");
        var elseBlock  = Ctx.CurrentFunction.AppendBasicBlock("else");
        var mergeBlock = Ctx.CurrentFunction.AppendBasicBlock("ifcont");

        Ctx.Builder.BuildCondBr(condition, thenBlock, elseBlock);
        Ctx.Builder.PositionAtEnd(thenBlock);

        if (context.block() != null) {
            VisitBlock(context.block());
        }

        thenBlock = Ctx.Builder.InsertBlock;
        if (thenBlock.Terminator.IsNull) {
            Ctx.Builder.BuildBr(mergeBlock);
        }

        Ctx.Builder.PositionAtEnd(elseBlock);

        if (context.elseStatement() != null) {
            Visit(context.elseStatement());
            // if (Ctx.Builder.InsertBlock.Terminator.IsNull) {
            // Ctx.Builder.BuildBr(mergeBlock);
            // }
        }

        if (elseBlock.Terminator.IsNull || elseBlock.Terminator == null) {
            var prevInsertPos = Ctx.Builder.InsertBlock;
            Ctx.Builder.PositionAtEnd(elseBlock);
            Ctx.Builder.BuildBr(mergeBlock);
            Ctx.Builder.PositionAtEnd(prevInsertPos);
        }

        if (Ctx.Builder.InsertBlock.Terminator == null || Ctx.Builder.InsertBlock.Terminator.IsNull) {
            Ctx.Builder.BuildBr(mergeBlock);
        } else {
            //Ctx.Builder.BuildBr(mergeBlock);
        }

        elseBlock = Ctx.Builder.InsertBlock;
        // if (elseBlock.Terminator.IsNull) {

        // }

        Ctx.Builder.PositionAtEnd(mergeBlock);

        return result;
    }

    /// <inheritdoc />
    public override CodeGenResult VisitValue(CustomLangParser.ValueContext context)
    {
        return context switch {
            _ when context.FLOAT() != null => new CodeGenResult().Add(
                LLVMValueRef.CreateConstRealOfString(LLVMTypeRef.Float, context.FLOAT().GetText())
            ),

            _ when context.STRING() != null => CodeGenResult.From(() =>
            {
                var str = context.STRING().GetText();
                str = str.Substring(1, str.Length - 2);
                var value       = LLVMValueRef.CreateConstString(str);
                var globalValue = Ctx.Module.AddGlobal(value.TypeOf, "str" + Ctx.StringCounter++).IsAGlobalValue;
                globalValue.Initializer = value;

                return globalValue;
            }),

            _ when context.INTEGER() != null => new CodeGenResult().Add(
                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong) int.Parse(context.INTEGER().GetText()))
            ),

            _ when context.id() != null => VisitId(context.id()),

            _ when context.callExpression() != null => VisitCallExpression(context.callExpression()),

            _ => base.VisitValue(context)
        };
    }

    public override CodeGenResult VisitPrimaryExpressionNonParen(CustomLangParser.PrimaryExpressionNonParenContext context)
    {
        return context switch {
            _ when context.value() != null      => VisitValue(context.value()),
            _ when context.expression() != null => VisitExpression(context.expression()),
            _                                   => base.VisitPrimaryExpressionNonParen(context)
        };
    }

    public override CodeGenResult VisitPrimaryExpression(CustomLangParser.PrimaryExpressionContext context)
    {
        return context switch {
            _ when context.value() != null      => VisitValue(context.value()),
            _ when context.expression() != null => VisitExpression(context.expression()),
            _                                   => base.VisitPrimaryExpression(context)
        };
    }


    public override CodeGenResult VisitMultiplicativeExpression(CustomLangParser.MultiplicativeExpressionContext context)
        => VisitBinaryExpression(new(context.expr, context.lhs, context.op, context.rhs));

    public override CodeGenResult VisitAdditiveExpression(CustomLangParser.AdditiveExpressionContext context)
        => VisitBinaryExpression(new(context.expr, context.lhs, context.op, context.rhs));

    public override CodeGenResult VisitRelationalExpression(CustomLangParser.RelationalExpressionContext context)
        => VisitBinaryExpression(new(context.expr, context.lhs, context.op, context.rhs));

    public override CodeGenResult VisitLogicalAndExpression(CustomLangParser.LogicalAndExpressionContext context)
        => VisitBinaryExpression(new(context.expr, context.lhs, context.op, context.rhs));

    public override CodeGenResult VisitLogicalOrExpression(CustomLangParser.LogicalOrExpressionContext context)
        => VisitBinaryExpression(new(context.expr, context.lhs, context.op, context.rhs));

    public override CodeGenResult VisitExpression(CustomLangParser.ExpressionContext context)
        => VisitBinaryExpression(new(context.expr, context.lhs, context.op, context.rhs));

    public override CodeGenResult VisitCallExpression(CustomLangParser.CallExpressionContext context)
    {
        var result = new CodeGenResult();

        var functionDeclaration = Scope.Current.LookupFunction(context.IDENTIFIER().GetText());

        var func = Ctx.Module.GetNamedFunction(functionDeclaration.Name);

        if (func == null) {
            throw new Exception($"Function {functionDeclaration.Name} not found");
        }


        if (func.ParamsCount != context.args().ChildCount && !functionDeclaration.IsVariadic) {
            throw new Exception($"Function {functionDeclaration.Name} expected {func.ParamsCount} parameters, but got {context.args().ChildCount}");
        }

        var args    = new List<LLVMValueRef>();
        var astArgs = context.args().primaryExpression();
        for (int i = 0; i < astArgs.Length; i++) {
            var expr = astArgs[i];
            args.Add(Visit(expr));
        }

        result.Add(
            Ctx.Builder.BuildCall2(functionDeclaration.LLVMType(), func, args.ToArray())
        );

        return result;
    }

    /// <inheritdoc />
    public override CodeGenResult VisitId(CustomLangParser.IdContext context)
    {
        var result = new CodeGenResult();

        var variableDeclaration = Scope.Current.LookupVariable(context.IDENTIFIER().GetText());
        if (!variableDeclaration.HasValue) {
            throw new Exception($"Variable {context.IDENTIFIER().GetText()} not found");
        }

        var variable = variableDeclaration.Value;

        result.Add(
            Ctx.Builder.BuildLoad2(variable.IsAAllocaInst.AllocatedType, variable, context.IDENTIFIER().GetText())
        );

        return result;
    }

    private CodeGenResult VisitBinaryExpression(ExpressionNode expr)
    {
        if (expr.op == null) {
            return base.Visit(expr.lhs);
        }

        var (success, left, right) = VisitOperands(expr.lhs!, expr.rhs!);
        return !success
            ? new CodeGenResult()
            : Expressions.GenerateExpression(OperatorsExtensions.FromIToken(expr.op), left, right);

        // (left, right) = TypeCoercion.Coerce(left, right);
        // if (operatorActions.TryGetValue(expr.op.Type, out var action)) {
        // return new CodeGenResult().Add(action(left, right));
        // }
        // throw new Exception("Unknown operator " + expr.op.Type);
    }

    private (bool success, LLVMValueRef left, LLVMValueRef right) VisitOperands(ParserRuleContext leftContext, ParserRuleContext rightContext)
    {
        var left     = Visit(leftContext);
        var right    = Visit(rightContext);
        var lhsToken = leftContext.Start;
        var rhsToken = rightContext.Start;

        if (left.Empty() || right.Empty()) {
            return (false, null, null);
        }

        return (true, left, right);
    }

}
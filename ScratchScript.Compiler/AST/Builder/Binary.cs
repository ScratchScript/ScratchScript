using Antlr4.Runtime;
using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor
{
    private IrNode? GenericBinaryExpressionHandler<T>(Func<T, IrBinaryOperator?> operatorConverter,
        ParserRuleContext context,
        T operatorContext,
        ScratchScriptParser.ExpressionContext leftContext, ScratchScriptParser.ExpressionContext rightContext)
        where T : ParserRuleContext
    {
        // get the operator
        if (operatorConverter(operatorContext) is not { } op)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, operatorContext);
            return null;
        }

        // get the left operand
        if (Visit(leftContext) is not IrExpressionNode left)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, leftContext);
            return null;
        }

        // get the right operand
        if (Visit(rightContext) is not IrExpressionNode right)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, rightContext);
            return null;
        }

        // left and right operands must be a number
        /*if (MustMatchTypeOrFail(left, ScratchType.Number, context, leftContext)) return null;
        if (MustMatchTypeOrFail(right, ScratchType.Number, context, rightContext)) return null;*/

        return new IrBinaryExpressionNode(op, left, right).WithContext(context);
    }

    public override IrNode? VisitBinaryBitwiseExpression(ScratchScriptParser.BinaryBitwiseExpressionContext context)
    {
        return GenericBinaryExpressionHandler(operatorContext =>
        {
            if (operatorContext.BitwiseAnd() != null) return IrBinaryOperator.BitwiseAnd;
            if (operatorContext.BitwiseOr() != null) return IrBinaryOperator.BitwiseOr;
            if (operatorContext.BitwiseXor() != null) return IrBinaryOperator.BitwiseXor;
            if (operatorContext.leftShift() != null) return IrBinaryOperator.BitwiseLeftShift;
            if (operatorContext.rightShift() != null) return IrBinaryOperator.BitwiseRightShift;
            return null;
        }, context, context.bitwiseOperators(), context.expression(0), context.expression(1));
    }

    public override IrNode? VisitBinaryCompareExpression(ScratchScriptParser.BinaryCompareExpressionContext context)
    {
        return GenericBinaryExpressionHandler(operatorContext =>
        {
            if (operatorContext.Equal() != null) return IrBinaryOperator.Equal;
            if (operatorContext.NotEqual() != null) return IrBinaryOperator.NotEqual;
            if (operatorContext.Greater() != null) return IrBinaryOperator.GreaterThan;
            if (operatorContext.GreaterOrEqual() != null) return IrBinaryOperator.GreaterOrEqualTo;
            if (operatorContext.Lesser() != null) return IrBinaryOperator.LessThan;
            if (operatorContext.LesserOrEqual() != null) return IrBinaryOperator.LessOrEqualTo;
            return null;
        }, context, context.compareOperators(), context.expression(0), context.expression(1));
    }

    public override IrNode? VisitBinaryMultiplyExpression(
        ScratchScriptParser.BinaryMultiplyExpressionContext context)
    {
        var result = GenericBinaryExpressionHandler(operatorContext =>
        {
            if (operatorContext.Multiply() != null) return IrBinaryOperator.Multiply;
            if (operatorContext.Divide() != null) return IrBinaryOperator.Divide;
            if (operatorContext.Power() != null) return IrBinaryOperator.Power;
            if (operatorContext.Modulus() != null) return IrBinaryOperator.Modulo;
            return null;
        }, context, context.multiplyOperators(), context.expression(0), context.expression(1));

        if (result is IrBinaryExpressionNode { Right: IrConstantExpressionNode { Value.Value: (double)0 } })
            DiagnosticReporter.Instance.Warning((int)ScratchScriptWarning.DivisionByZero, context, context);
        return result;
    }

    public override IrNode?
        VisitBinaryBooleanExpression(ScratchScriptParser.BinaryBooleanExpressionContext context)
    {
        return GenericBinaryExpressionHandler(operatorContext =>
        {
            if (operatorContext.And() != null) return IrBinaryOperator.And;
            if (operatorContext.Or() != null) return IrBinaryOperator.Or;
            return null;
        }, context, context.booleanOperators(), context.expression(0), context.expression(1));
    }


    public override IrNode? VisitBinaryAddExpression(ScratchScriptParser.BinaryAddExpressionContext context)
    {
        var result = GenericBinaryExpressionHandler(operatorContext =>
            {
                if (operatorContext.Plus() != null) return IrBinaryOperator.Add;
                if (operatorContext.Minus() != null) return IrBinaryOperator.Subtract;
                return null;
            }, context, context.addOperators(),
            context.expression(0), context.expression(1));

        /*if (result is IrBinaryExpressionNode { Operator: IrBinaryOperator.Add } binaryExpressionNode &&
            (DetermineExpressionType(binaryExpressionNode.Left) == ScratchType.String ||
             DetermineExpressionType(binaryExpressionNode.Right) == ScratchType.String))
            result = binaryExpressionNode with { Operator = IrBinaryOperator.Join };*/

        return result;
    }
}
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.GeneratedVisitor;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.Implementation;

public partial class ScratchCEVisitor
{
    public override TypedValue VisitBinaryAddExpression(ScratchCEParser.BinaryAddExpressionContext context)
    {
        var op = context.addOperators().GetText();
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        if (op == null) throw new Exception("The operator was null.");
        if (left == null) throw new Exception("The left operand was null.");
        if (right == null) throw new Exception("The right operand was null.");

        return op switch
        {
            "+" => new TypedValue(left.Value.CastOrThrow<double>() + right.Value.CastOrThrow<double>(),
                ScratchType.Number),
            "-" => new TypedValue(left.Value.CastOrThrow<double>() - right.Value.CastOrThrow<double>(),
                ScratchType.Number),
            "~" => new TypedValue(left.Value.CastOrThrow<string>() + right.Value.CastOrThrow<string>(),
                ScratchType.String),
            _ => throw new ArgumentOutOfRangeException(nameof(op), $"""The add operator was invalid ("{op}").""")
        };
    }

    public override TypedValue VisitBinaryMultiplyExpression(ScratchCEParser.BinaryMultiplyExpressionContext context)
    {
        var op = context.multiplyOperators().GetText();
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        if (op == null) throw new Exception("The operator was null.");
        if (left == null) throw new Exception("The left operand was null.");
        if (right == null) throw new Exception("The right operand was null.");

        if (right.Value.CastOrThrow<double>() == 0 && op == "/")
            return new TypedValue(double.PositiveInfinity, ScratchType.Number);

        return op switch
        {
            "*" => new TypedValue(left.Value.CastOrThrow<double>() * right.Value.CastOrThrow<double>(),
                ScratchType.Number),
            "/" => new TypedValue(left.Value.CastOrThrow<double>() / right.Value.CastOrThrow<double>(),
                ScratchType.Number),
            "%" => new TypedValue(left.Value.CastOrThrow<double>() % right.Value.CastOrThrow<double>(),
                ScratchType.Number),
            _ => throw new ArgumentOutOfRangeException(nameof(op), $"""The multiply operator was invalid ("{op}").""")
        };
    }

    public override TypedValue VisitBinaryBooleanExpression(ScratchCEParser.BinaryBooleanExpressionContext context)
    {
        var op = context.booleanOperators().GetText();
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        if (op == null) throw new Exception("The operator was null.");
        if (left == null) throw new Exception("The left operand was null.");
        if (right == null) throw new Exception("The right operand was null.");

        return op switch
        {
            "&&" => new TypedValue(left.Value.CastOrThrow<bool>() && right.Value.CastOrThrow<bool>(),
                ScratchType.Boolean),
            "||" => new TypedValue(left.Value.CastOrThrow<bool>() || right.Value.CastOrThrow<bool>(),
                ScratchType.Boolean),
            _ => throw new ArgumentOutOfRangeException(nameof(op), $"""The boolean operator was invalid ("{op}").""")
        };
    }

    public override TypedValue VisitBinaryCompareExpression(ScratchCEParser.BinaryCompareExpressionContext context)
    {
        var op = context.compareOperators().GetText();
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        if (op == null) throw new Exception("The operator was null.");
        if (left == null) throw new Exception("The left operand was null.");
        if (right == null) throw new Exception("The right operand was null.");

        return op switch
        {
            "==" => new TypedValue(left == right && left.Type == right.Type, ScratchType.Boolean),
            "!=" => new TypedValue(left != right || left.Type != right.Type, ScratchType.Boolean),
            ">" => new TypedValue(left.Value.CastOrThrow<double>() > right.Value.CastOrThrow<double>(),
                ScratchType.Boolean),
            ">=" => new TypedValue(left.Value.CastOrThrow<double>() >= right.Value.CastOrThrow<double>(),
                ScratchType.Boolean),
            "<" => new TypedValue(left.Value.CastOrThrow<double>() < right.Value.CastOrThrow<double>(),
                ScratchType.Boolean),
            "<=" => new TypedValue(left.Value.CastOrThrow<double>() <= right.Value.CastOrThrow<double>(),
                ScratchType.Boolean),
            _ => throw new ArgumentOutOfRangeException(nameof(op), $"""The comparison operator was invalid ("{op}").""")
        };
    }

    public override TypedValue VisitBinaryBitwiseExpression(ScratchCEParser.BinaryBitwiseExpressionContext context)
    {
        var op = context.bitwiseOperators().GetText();
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        if (op == null) throw new Exception("The operator was null.");
        if (left == null) throw new Exception("The left operand was null.");
        if (right == null) throw new Exception("The right operand was null.");

        var leftTruncated = (long)Math.Truncate(left.Value.CastOrThrow<double>());
        var rightTruncated = (long)Math.Truncate(right.Value.CastOrThrow<double>());

        return op switch
        {
            "|" => new TypedValue(leftTruncated | rightTruncated, ScratchType.Number),
            "&" => new TypedValue(leftTruncated & rightTruncated, ScratchType.Number),
            "^" => new TypedValue(leftTruncated ^ rightTruncated, ScratchType.Number),
            "<<" => new TypedValue(leftTruncated << (int)rightTruncated, ScratchType.Number),
            ">>" => new TypedValue(leftTruncated >> (int)rightTruncated, ScratchType.Number),
            _ => throw new ArgumentOutOfRangeException(nameof(op), $"""The bitwise operator was invalid ("{op}").""")
        };
    }
}
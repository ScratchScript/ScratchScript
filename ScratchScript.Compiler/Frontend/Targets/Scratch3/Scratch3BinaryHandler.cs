using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

// ReSharper disable SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3BinaryHandler(char commandSeparator) : IBinaryHandler
{
    public ExpressionValue GetBinaryMultiplyExpression(ref IScope scope, MultiplyOperators op, ExpressionValue left,
        ExpressionValue right)
    {
        var dependencies = left.Dependencies.Combine(commandSeparator, right.Dependencies);
        var cleanup = left.Cleanup.Combine(commandSeparator, right.Cleanup);

        return op switch
        {
            MultiplyOperators.Multiply => new ExpressionValue($"* {left.Value} {right.Value}", ScratchType.Number,
                dependencies, cleanup),
            MultiplyOperators.Divide => new ExpressionValue($"/ {left.Value} {right.Value}", ScratchType.Number,
                dependencies, cleanup),
            MultiplyOperators.Modulus => throw new NotImplementedException(),
            MultiplyOperators.Power => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }

    public ExpressionValue GetBinaryStringEquationExpression(ref IScope scope, ExpressionValue left,
        ExpressionValue right)
    {
        throw new NotImplementedException();
    }

    public ExpressionValue GetBinaryNumberEquationExpression(ref IScope scope, ExpressionValue left,
        ExpressionValue right)
    {
        // TODO: account for float calculations later
        return new ExpressionValue($"== {left.Value} {right.Value}", ScratchType.Boolean,
            left.Dependencies.Combine(commandSeparator, right.Dependencies),
            left.Cleanup.Combine(commandSeparator, right.Cleanup));
    }

    public ExpressionValue GetBinaryBitwiseExpression(ref IScope scope, BitwiseOperators op, ExpressionValue left,
        ExpressionValue right)
    {
        throw new NotImplementedException();
    }

    public ExpressionValue GetBinaryNumberComparisonExpression(ref IScope scope, CompareOperators op,
        ExpressionValue left,
        ExpressionValue right)
    {
        var dependencies = left.Dependencies.Combine(commandSeparator, right.Dependencies);
        var cleanup = left.Cleanup.Combine(commandSeparator, right.Cleanup);

        return op switch
        {
            CompareOperators.LessThan => new ExpressionValue($"< {left.Value} {right.Value}", ScratchType.Boolean,
                dependencies, cleanup),
            CompareOperators.GreaterThan => new ExpressionValue($"> {left.Value} {right.Value}", ScratchType.Boolean,
                dependencies, cleanup),
            CompareOperators.LessThanOrEqual => new ExpressionValue(
                $"|| < {left.Value} {right.Value} == {left.Value} {right.Value}", ScratchType.Boolean, dependencies,
                cleanup),
            CompareOperators.GreaterThanOrEqual => new ExpressionValue(
                $"|| > {left.Value} {right.Value} == {left.Value} {right.Value}", ScratchType.Boolean, dependencies,
                cleanup),
            _ => throw new Exception(
                "GetBinaryNumberComparisonExpression only expects LessThan, GreaterThan, LessThanOrEqual and GreaterThanOrEqual operators.")
        };
    }
}
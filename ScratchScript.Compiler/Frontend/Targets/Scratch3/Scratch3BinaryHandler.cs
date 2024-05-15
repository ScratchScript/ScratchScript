using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3BinaryHandler(char commandSeparator) : IBinaryHandler
{
    public ExpressionValue GetBinaryMultiplyExpression(ref Scope scope, MultiplyOperators op, ExpressionValue left,
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
}
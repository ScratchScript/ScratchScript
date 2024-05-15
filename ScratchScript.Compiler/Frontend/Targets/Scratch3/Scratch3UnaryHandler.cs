using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3UnaryHandler : IUnaryHandler
{
    public ExpressionValue GetUnaryExpression(AddOperator op, ExpressionValue expression)
    {
        return new ExpressionValue($"* {(op == AddOperator.Plus ? 1 : -1)} {expression.Value}", ScratchType.Number,
            expression.Dependencies, expression.Cleanup);
    }
}
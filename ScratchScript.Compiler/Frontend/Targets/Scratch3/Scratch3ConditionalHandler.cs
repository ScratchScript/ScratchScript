using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3ConditionalHandler : IConditionalHandler
{
    public ExpressionValue GetEqualityExpression(ExpressionValue value)
    {
        if (value is IdentifierExpressionValue identifierExpressionValue)
            return new ExpressionValue($"== {identifierExpressionValue.Value} \"true\"", ScratchType.Boolean,
                value.Dependencies, value.Cleanup);

        return value;
    }
}
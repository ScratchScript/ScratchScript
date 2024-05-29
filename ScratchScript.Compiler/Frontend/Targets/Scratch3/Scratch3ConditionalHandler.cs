using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3ConditionalHandler : IConditionalHandler
{
    public ExpressionValue GetEqualityExpression(ExpressionValue value)
    {
        if (value is IdentifierExpressionValue identifierExpressionValue)
            return new ExpressionValue($"== {identifierExpressionValue.Value} \"true\"", ScratchType.Boolean,
                value.Dependencies, value.Cleanup);
        if ((value.Value?.ToString() ?? "").Replace("\"", "") is "true" or "false")
            return new ExpressionValue($"== {value.Value} \"true\"", ScratchType.Boolean);

        return value;
    }
}
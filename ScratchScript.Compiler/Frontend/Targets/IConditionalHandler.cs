using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IConditionalHandler
{
    public ExpressionValue GetEqualityExpression(ExpressionValue value);
}
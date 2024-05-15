using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IBinaryHandler
{
    public ExpressionValue GetBinaryMultiplyExpression(ref Scope scope, MultiplyOperators op, ExpressionValue left,
        ExpressionValue right);
}
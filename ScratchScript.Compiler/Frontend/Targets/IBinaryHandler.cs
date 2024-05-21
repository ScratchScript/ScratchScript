using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IBinaryHandler
{
    public ExpressionValue GetBinaryMultiplyExpression(ref IScope scope, MultiplyOperators op, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryStringEquationExpression(ref IScope scope, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryNumberEquationExpression(ref IScope scope, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryNumberComparisonExpression(ref IScope scope, CompareOperators op,
        ExpressionValue left, ExpressionValue right);

    public ExpressionValue GetBinaryBitwiseExpression(ref IScope scope, BitwiseOperators op, ExpressionValue left,
        ExpressionValue right);
}
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IBinaryHandler
{
    public ExpressionValue GetBinaryMultiplyExpression(IScope scope, MultiplyOperators op, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryStringEquationExpression(IScope scope, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryNumberEquationExpression(IScope scope, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryNumberComparisonExpression(IScope scope, CompareOperators op,
        ExpressionValue left, ExpressionValue right);

    public ExpressionValue GetBinaryBitwiseExpression(IScope scope, BitwiseOperators op, ExpressionValue left,
        ExpressionValue right);
}
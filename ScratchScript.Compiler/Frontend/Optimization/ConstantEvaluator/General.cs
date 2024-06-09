using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Optimization.ConstEvaluator;

public static class ConstEvaluator
{
    public static bool IsConstant(TypedValue value)
    {
        if (value is ExpressionValue { ContainsIntermediateRepresentation: true } or StatementValue or ScopeValue)
            return false;
        return true;
    }
}
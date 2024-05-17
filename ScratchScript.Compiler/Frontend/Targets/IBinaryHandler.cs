﻿using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IBinaryHandler
{
    public ExpressionValue GetBinaryMultiplyExpression(ref Scope scope, MultiplyOperators op, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryStringEquationExpression(ref Scope scope, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryNumberEquationExpression(ref Scope scope, ExpressionValue left,
        ExpressionValue right);

    public ExpressionValue GetBinaryNumberComparisonExpression(ref Scope scope, CompareOperators op,
        ExpressionValue left, ExpressionValue right);

    public ExpressionValue GetBinaryBitwiseExpression(ref Scope scope, BitwiseOperators op, ExpressionValue left,
        ExpressionValue right);
}
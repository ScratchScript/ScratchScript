using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.GeneratedVisitor;
using ScratchScript.Compiler.Types;
using Exception = System.Exception;

namespace ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.Implementation;

public partial class ScratchCEVisitor
{
    public override TypedValue? VisitArrayAccessExpression(ScratchCEParser.ArrayAccessExpressionContext context)
    {
        var operand = Visit(context.expression(0));
        var index = Visit(context.expression(1));

        if (operand == null) throw new Exception("The array was null.");
        if (index == null) throw new Exception("The index was null.");
        if (operand.Type.Kind != ScratchTypeKind.List)
            throw new Exception("Cannot perform array access on a non-array type.");
        if (index.Type != ScratchType.Number) throw new Exception("The index must be a number.");

        return operand.Value.CastOrThrow<List<TypedValue>>()[(int)index.Value.CastOrThrow<double>()];
    }

    public override TypedValue? VisitIndexOfExpression(ScratchCEParser.IndexOfExpressionContext context)
    {
        var operand = Visit(context.expression(0));
        var what = Visit(context.expression(1));
        
        if (operand == null) throw new Exception("The array was null.");
        if (what == null) throw new Exception("The index was null.");
        if (operand.Type.Kind != ScratchTypeKind.List)
            throw new Exception("Cannot perform item search on a non-array type.");
        if (what.Type != operand.Type.ChildType)
            throw new Exception("The search item did not match the array's type.");

        return new TypedValue(operand.Value.CastOrThrow<List<TypedValue>>().IndexOf(what), ScratchType.Number);
    }
}
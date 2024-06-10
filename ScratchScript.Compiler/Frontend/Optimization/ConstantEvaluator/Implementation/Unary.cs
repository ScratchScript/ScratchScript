using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.GeneratedVisitor;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.Implementation;

public partial class ScratchCEVisitor
{
    public override TypedValue VisitNotExpression(ScratchCEParser.NotExpressionContext context)
    {
        var value = Visit(context.expression());
        if (value == null) throw new Exception("The value was null.");
        return new TypedValue(!value.Value.CastOrThrow<bool>(), ScratchType.Boolean);
    }
}
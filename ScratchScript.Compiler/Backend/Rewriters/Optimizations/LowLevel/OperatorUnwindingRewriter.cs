using ScratchScript.Compiler.Backend.Information;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Backend.Rewriters.TargetLowering;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Backend.Rewriters.Optimizations.LowLevel;

public class OperatorUnwindingRewriter : IrRewriter
{
    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        var result = (IrBinaryExpressionNode)base.VisitBinaryExpression(node);

        IrBinaryOperator SeparateOperator(IrBinaryOperator op) => op switch
        {
            IrBinaryOperator.NotEqual => IrBinaryOperator.Equal,
            IrBinaryOperator.LessOrEqualTo => IrBinaryOperator.LessThan,
            IrBinaryOperator.GreaterOrEqualTo => IrBinaryOperator.GreaterThan,
            _ => op
        };

        return node.Operator switch
        {
            IrBinaryOperator.LessOrEqualTo or IrBinaryOperator.GreaterOrEqualTo => new IrBinaryExpressionNode(
                IrBinaryOperator.Or, new IrBinaryExpressionNode(IrBinaryOperator.Equal, result.Left, result.Right),
                new IrBinaryExpressionNode(SeparateOperator(node.Operator), result.Left, result.Right)),
            IrBinaryOperator.NotEqual => new IrUnaryExpressionNode(IrUnaryOperator.Not,
                new IrBinaryExpressionNode(IrBinaryOperator.Equal, result.Left, result.Right)),
            _ => result
        };
    }

    public override IrNode VisitStackPointerExpressionNode(IrStackPointerExpressionNode node)
    {
        return Scratch3CommandHelper.ItemAt(ReservedNames.Stack, node.Offset == 0
            ? Scratch3CommandHelper.LengthOf(ReservedNames.Stack)
            : new IrBinaryExpressionNode(IrBinaryOperator.Subtract, Scratch3CommandHelper.LengthOf(ReservedNames.Stack),
                new IrConstantExpressionNode(TypedValue.Number(node.Offset))));
    }
}
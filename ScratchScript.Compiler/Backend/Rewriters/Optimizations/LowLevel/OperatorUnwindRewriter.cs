using ScratchScript.Compiler.Backend.Representation;

namespace ScratchScript.Compiler.Backend.Rewriters.LowLevel;

public class OperatorUnwindRewriter: IrRewriter
{
    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        var result = (IrBinaryExpressionNode)base.VisitBinaryExpression(node);
        return node.Operator == IrBinaryOperator.NotEqual
            ? new IrUnaryExpressionNode(IrUnaryOperator.Not,
                new IrBinaryExpressionNode(IrBinaryOperator.Equal, result.Left, result.Right))
            : result;
    }
}
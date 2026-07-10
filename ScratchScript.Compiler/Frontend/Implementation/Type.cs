using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public ScratchType DetermineExpressionType(IrExpressionNode node)
        => node switch
        {
            IrBinaryExpressionNode irBinaryExpressionNode => irBinaryExpressionNode.Operator == IrBinaryOperator.Join
                ? ScratchType.String
                : irBinaryExpressionNode.Operator > IrBinaryOperator.Join
                    ? ScratchType.Boolean
                    : ScratchType.Number,
            IrConstantExpressionNode irConstantExpressionNode => irConstantExpressionNode.Value.Type,
            IrFunctionArgumentExpressionNode irFunctionArgumentExpressionNode => _scope?
                .GetVariable(irFunctionArgumentExpressionNode.Name)?.Type ?? ScratchType.Unknown,
            IrFunctionCallExpressionNode irFunctionCallExpressionNode => throw new NotImplementedException(),
            IrGlobalListIdentifierExpressionNode irGlobalListIdentifierExpressionNode =>
                throw new NotImplementedException(),
            IrGlobalVariableIdentifierExpressionNode irGlobalVariableIdentifierExpressionNode =>
                throw new NotImplementedException(),
            IrLocalVariableIdentifierExpressionNode irLocalVariableIdentifierExpressionNode =>
                _scope?.GetVariable(irLocalVariableIdentifierExpressionNode.Name)
                    ?.Type ?? ScratchType.Unknown,
            IrParenthesizedExpressionNode irParenthesizedExpressionNode => DetermineExpressionType(
                irParenthesizedExpressionNode.Expression),
            IrShadowExpressionNode irShadowExpressionNode => irShadowExpressionNode.ExpectedType ?? ScratchType.Unknown,
            IrUnaryExpressionNode irUnaryExpressionNode => ScratchType.Number,
            _ => throw new ArgumentOutOfRangeException(nameof(node))
        };
}
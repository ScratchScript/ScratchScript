using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.Rewriters.Optimization;

public class ConstantEvaluationRewriter : IrRewriter
{
    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        if (Visit(node.Left) is not IrExpressionNode left) throw new Exception();
        if (Visit(node.Right) is not IrExpressionNode right) throw new Exception();

        if (left is not IrConstantExpressionNode constLeft || right is not IrConstantExpressionNode constRight)
            return node with { Left = left, Right = right };

        if (node.Operator < IrBinaryOperator.Join)
        {
            var leftValue = constLeft.Extract<double>();
            var rightValue = constRight.Extract<double>();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return new IrConstantExpressionNode(TypedValue.Number(node.Operator switch
            {
                IrBinaryOperator.Add => leftValue + rightValue,
                IrBinaryOperator.Subtract => leftValue - rightValue,
                IrBinaryOperator.Multiply => leftValue * rightValue,
                IrBinaryOperator.Divide => leftValue / rightValue,
                IrBinaryOperator.Modulo => leftValue % rightValue,
                IrBinaryOperator.Power => Math.Pow(leftValue, rightValue),
                IrBinaryOperator.BitwiseOr => (long)leftValue | (long)rightValue,
                IrBinaryOperator.BitwiseAnd => (long)leftValue & (long)rightValue,
                IrBinaryOperator.BitwiseXor => (long)leftValue ^ (long)rightValue,
                IrBinaryOperator.BitwiseLeftShift => (long)leftValue << (int)rightValue,
                IrBinaryOperator.BitwiseRightShift => (long)leftValue >> (int)rightValue,
                _ => throw new ArgumentOutOfRangeException()
            }));
        }

        if (node.Operator == IrBinaryOperator.Join)
            return new IrConstantExpressionNode(
                TypedValue.String(constLeft.Extract<string>() + constRight.Extract<string>()));

        if (node.Operator is >= IrBinaryOperator.And and <= IrBinaryOperator.Xor)
        {
            var leftValue = constLeft.Extract<bool>();
            var rightValue = constRight.Extract<bool>();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return new IrConstantExpressionNode(TypedValue.Boolean(node.Operator switch
            {
                IrBinaryOperator.And => leftValue && rightValue,
                IrBinaryOperator.Or => leftValue || rightValue,
                IrBinaryOperator.Xor => leftValue ^ rightValue,
                _ => throw new ArgumentOutOfRangeException()
            }));
        }

        if (node.Operator is IrBinaryOperator.Equal or IrBinaryOperator.NotEqual)
        {
            var equal = Equals(constLeft.Value.Value, constRight.Value.Value);
            return new IrConstantExpressionNode(
                TypedValue.Boolean(node.Operator == IrBinaryOperator.Equal ? equal : !equal));
        }

        if (node.Operator >= IrBinaryOperator.LessThan)
        {
            var leftValue = constLeft.Extract<double>();
            var rightValue = constRight.Extract<double>();
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return new IrConstantExpressionNode(TypedValue.Boolean(node.Operator switch
            {
                IrBinaryOperator.LessThan => leftValue < rightValue,
                IrBinaryOperator.LessOrEqualTo => leftValue <= rightValue,
                IrBinaryOperator.GreaterThan => leftValue > rightValue,
                IrBinaryOperator.GreaterOrEqualTo => leftValue >= rightValue,
                _ => throw new ArgumentOutOfRangeException()
            }));
        }

        throw new ArgumentOutOfRangeException();
    }
}
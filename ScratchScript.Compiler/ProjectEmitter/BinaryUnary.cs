using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.ProjectEmitter.Blocks;
using ScratchScript.Compiler.ProjectEmitter.Models;

namespace ScratchScript.Compiler.ProjectEmitter;

public partial class ScratchScriptProjectEmitter
{
    public override object? VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        var operatorBlock = CreateBinaryOperatorBlock(node.Operator);
        var left = Visit(node.Left);
        var right = Visit(node.Right);
        if (left == null) throw new Exception("The left operand passed to VisitBinaryExpression was null.");
        if (right == null) throw new Exception("The right operand passed to VisitBinaryExpression was null.");

        var prefix = node.Operator == IrBinaryOperator.Join ? "STRING" :
            node.Operator > IrBinaryOperator.Join ? "OPERAND" : "NUM";
        operatorBlock.Inputs[$"{prefix}1"] =
            left is Block leftBlock ? CreateInput(leftBlock, operatorBlock) : CreateInput(left);
        operatorBlock.Inputs[$"{prefix}2"] = right is Block rightBlock
            ? CreateInput(rightBlock, operatorBlock)
            : CreateInput(right);

        return operatorBlock;
    }

    public override object? VisitUnaryExpression(IrUnaryExpressionNode node)
    {
        if (node.Operator != IrUnaryOperator.Not) throw new NotImplementedException();
        var operand = Visit(node.Operand);
        if (operand == null) throw new Exception("The operand of VisitUnaryExpression was null.");
        var block = new Block { Opcode = Operators.Not, Id = GenerateBlockId(Operators.Not) };
        block.Inputs["OPERAND"] =
            operand is Block operandBlock ? CreateInput(operandBlock, block) : CreateInput(operand);
        Target.Blocks[block.Id] = block;
        return block;
    }

    private Block CreateBinaryOperatorBlock(IrBinaryOperator op)
    {
        var block = new Block
        {
            Opcode = op switch
            {
                IrBinaryOperator.Add => Operators.Add,
                IrBinaryOperator.Subtract => Operators.Subtract,
                IrBinaryOperator.Multiply => Operators.Multiply,
                IrBinaryOperator.Divide => Operators.Divide,
                IrBinaryOperator.Modulo => Operators.Modulus,
                IrBinaryOperator.Power => throw new NotImplementedException(),
                IrBinaryOperator.Join => Operators.Join,
                IrBinaryOperator.And => Operators.And,
                IrBinaryOperator.Or => Operators.Or,
                IrBinaryOperator.Xor => throw new NotImplementedException(),
                IrBinaryOperator.Equal => Operators.Equals,
                IrBinaryOperator.NotEqual => throw new NotImplementedException(),
                IrBinaryOperator.LessThan => Operators.LessThan,
                IrBinaryOperator.GreaterThan => Operators.GreaterThan,
                _ => throw new ArgumentOutOfRangeException()
            }
        };
        block.Id = GenerateBlockId(block.Opcode);
        Target.Blocks[block.Id] = block;
        return block;
    }
}
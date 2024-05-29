using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor
{
    public override object VisitBinaryAddExpression(ScratchIRParser.BinaryAddExpressionContext context)
    {
        return VisitMathematicalExpression(Visit(context.addOperators()), Visit(context.expression(0)),
            Visit(context.expression(1)));
    }

    public override object VisitBinaryMultiplyExpression(ScratchIRParser.BinaryMultiplyExpressionContext context)
    {
        return VisitMathematicalExpression(Visit(context.multiplyOperators()), Visit(context.expression(0)),
            Visit(context.expression(1)));
    }

    public override object VisitBinaryBooleanExpression(ScratchIRParser.BinaryBooleanExpressionContext context)
    {
        return VisitConditionExpression(Visit(context.booleanOperators()), Visit(context.expression(0)),
            Visit(context.expression(1)));
    }

    public override object VisitBinaryCompareExpression(ScratchIRParser.BinaryCompareExpressionContext context)
    {
        return VisitConditionExpression(Visit(context.compareOperators()), Visit(context.expression(0)),
            Visit(context.expression(1)));
    }

    private Block VisitMathematicalExpression(object? op, object? left, object? right)
    {
        if (op is not Block operatorBlock)
            throw new Exception("The operator block passed to VisitMathematicalExpression was not a block.");
        if (left == null) throw new Exception("The left operand passed to VisitMathematicalExpression was null.");
        if (right == null) throw new Exception("The right operand passed to VisitMathematicalExpression was null.");

        if (operatorBlock.Opcode == Operators.Join)
        {
            operatorBlock.Inputs["STRING1"] =
                left is Block leftBlock ? CreateInput(leftBlock, operatorBlock) : CreateInput(left);
            operatorBlock.Inputs["STRING2"] = right is Block rightBlock
                ? CreateInput(rightBlock, operatorBlock)
                : CreateInput(right);
        }
        else
        {
            operatorBlock.Inputs["NUM1"] =
                left is Block leftBlock ? CreateInput(leftBlock, operatorBlock) : CreateInput(left);
            operatorBlock.Inputs["NUM2"] = right is Block rightBlock
                ? CreateInput(rightBlock, operatorBlock)
                : CreateInput(right);
        }

        return operatorBlock;
    }

    private Block VisitConditionExpression(object? op, object? left, object? right)
    {
        if (op is not Block operatorBlock)
            throw new Exception("The operator block passed to VisitConditionExpression was not a block.");
        if (left == null) throw new Exception("The left operand passed to VisitConditionExpression was null.");
        if (right == null) throw new Exception("The right operand passed to VisitConditionExpression was null.");

        operatorBlock.Inputs["OPERAND1"] =
            left is Block leftBlock ? CreateInput(leftBlock, operatorBlock) : CreateInput(left);
        operatorBlock.Inputs["OPERAND2"] = right is Block rightBlock
            ? CreateInput(rightBlock, operatorBlock)
            : CreateInput(right);

        return operatorBlock;
    }
}
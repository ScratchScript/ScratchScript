using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.GeneratedVisitor;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor
{
    public override object VisitNotExpression(ScratchIRParser.NotExpressionContext context)
    {
        var expression = Visit(context.expression());
        if (expression == null) throw new Exception("An expression in VisitNotExpression was null.");

        var block = new Block { Opcode = Operators.Not, Id = GenerateBlockId(Operators.Not) };
        block.Inputs["OPERAND"] = expression is Block expressionBlock
            ? CreateInput(expressionBlock, block)
            : CreateInput(expression);

        return block;
    }
}
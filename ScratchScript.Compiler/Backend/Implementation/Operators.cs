using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.GeneratedVisitor;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor
{
    public override object VisitAddOperators(ScratchIRParser.AddOperatorsContext context)
    {
        return CreateOperatorBlock(context.GetText());
    }

    public override object VisitMultiplyOperators(ScratchIRParser.MultiplyOperatorsContext context)
    {
        return CreateOperatorBlock(context.GetText());
    }

    public override object VisitBooleanOperators(ScratchIRParser.BooleanOperatorsContext context)
    {
        return CreateOperatorBlock(context.GetText());
    }

    public override object VisitCompareOperators(ScratchIRParser.CompareOperatorsContext context)
    {
        return CreateOperatorBlock(context.GetText());
    }

    private Block CreateOperatorBlock(string op)
    {
        var block = new Block
        {
            Opcode = op switch
            {
                "+" => Operators.Add,
                "-" => Operators.Subtract,
                "~" => Operators.Join,
                "*" => Operators.Multiply,
                "/" => Operators.Divide,
                "%" => Operators.Modulus,
                "&&" => Operators.And,
                "||" => Operators.Or,
                ">" => Operators.GreaterThan,
                "<" => Operators.LessThan,
                "==" => Operators.Equals,
                _ => throw new ArgumentOutOfRangeException()
            }
        };
        block.Id = GenerateBlockId(block.Opcode);

        Target.Blocks[block.Id] = block;
        return block;
    }
}
using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.GeneratedVisitor;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor
{
    public override object? VisitRepeatCommand(ScratchIRParser.RepeatCommandContext context)
    {
        var times = Visit(context.expression());
        if (times == null) throw new Exception("The expression passed to VisitRepeatCommand was null.");

        var block = new Block { Opcode = Control.Repeat, Id = GenerateBlockId(Control.Repeat) };
        block.Inputs["TIMES"] = times is Block timesBlock ? CreateInput(timesBlock, block) : CreateInput(times);

        var stack = VisitCommands(context.command());
        AttachStackToBlock(block, stack, "SUBSTACK");

        return block;
    }

    public override object? VisitWhileCommand(ScratchIRParser.WhileCommandContext context)
    {
        var condition = Visit(context.expression());
        if (condition is not Block conditionBlock)
            throw new Exception("VisitWhileStatement expected the condition expression to be a block, but it wasn't.");

        var block = new Block { Opcode = Control.While, Id = GenerateBlockId(Control.While) };
        block.Inputs["CONDITION"] = CreateInput(conditionBlock, block);

        var stack = VisitCommands(context.command());
        AttachStackToBlock(block, stack, "SUBSTACK");

        return block;
    }

    public override object VisitIfStatement(ScratchIRParser.IfStatementContext context)
    {
        var condition = Visit(context.expression());
        if (condition is not Block conditionBlock)
            throw new Exception("VisitIfStatement expected the condition expression to be a block, but it wasn't.");

        var block = new Block { Opcode = context.elseIfStatement() != null ? Control.IfElse : Control.If };
        block.Id = GenerateBlockId(block.Opcode);
        block.Inputs["CONDITION"] = CreateInput(conditionBlock, block);

        var stack = VisitCommands(context.command());
        AttachStackToBlock(block, stack, "SUBSTACK");

        if (context.elseIfStatement() != null)
        {
            var elseValue = Visit(context.elseIfStatement());
            switch (elseValue)
            {
                case List<Block> elseStack:
                {
                    AttachStackToBlock(block, elseStack, "SUBSTACK2");
                    break;
                }
                case Block elseBlock:
                {
                    elseBlock.Parent = block.Id;
                    block.Inputs["SUBSTACK2"] = CreateInput(elseBlock, block);
                    break;
                }
            }
        }

        return block;
    }

    public override object? VisitElseIfStatement(ScratchIRParser.ElseIfStatementContext context)
    {
        if (context.ifStatement() != null) return Visit(context.ifStatement());
        if (context.command() != null) return VisitCommands(context.command());
        return null;
    }
}
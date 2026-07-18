using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.ProjectEmitter.Blocks;
using ScratchScript.Compiler.ProjectEmitter.Models;

namespace ScratchScript.Compiler.ProjectEmitter;

public partial class ScratchScriptProjectEmitter
{
    public override object? VisitWhileCommand(IrWhileCommandNode node)
    {
        var condition = Visit(node.Condition);
        if (condition is not Block conditionBlock)
            throw new Exception("VisitWhileCommand expected the condition expression to be a block, but it wasn't.");

        var block = new Block { Opcode = Control.While, Id = GenerateBlockId(Control.While) };
        block.Inputs["CONDITION"] = CreateInput(conditionBlock, block);

        var stack = VisitScope(node.Body.Scope);
        AttachStackToBlock(block, stack, "SUBSTACK");

        return block;
    }

    public override object? VisitRepeatCommand(IrRepeatCommandNode node)
    {
        var times = Visit(node.Times);
        if (times == null) throw new Exception("The expression passed to VisitRepeatCommand was null.");

        var block = new Block { Opcode = Control.Repeat, Id = GenerateBlockId(Control.Repeat) };
        block.Inputs["TIMES"] = times is Block timesBlock ? CreateInput(timesBlock, block) : CreateInput(times);

        var stack = VisitScope(node.Body.Scope);
        AttachStackToBlock(block, stack, "SUBSTACK");

        return block;
    }

    public override object? VisitIfCommand(IrIfCommandNode node)
    {
        var condition = Visit(node.Condition);
        if (condition is not Block conditionBlock)
            throw new Exception("VisitIfCommand expected the condition expression to be a block, but it wasn't.");

        var block = new Block { Opcode = node.Alternate != null ? Control.IfElse : Control.If };
        block.Id = GenerateBlockId(block.Opcode);
        block.Inputs["CONDITION"] = CreateInput(conditionBlock, block);

        var stack = VisitScope(node.Body.Scope);
        AttachStackToBlock(block, stack, "SUBSTACK");
        if (node.Alternate != null)
        {
            var elseStack = VisitScope(node.Alternate.Scope);
            AttachStackToBlock(block, elseStack, "SUBSTACK2");
        }

        return block;
    }
}
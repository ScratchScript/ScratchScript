using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Emitter;

public partial class ScratchScriptProjectEmitter(string SourceHash) : IrBaseVisitor<object?>
{
    public readonly Target Target = new();
    private Block? _parent;

    private List<Block> VisitScope(Scope scope)
    {
        var lastParent = _parent;
        _parent = null;

        var blocks = new List<Block>();
        foreach (var command in scope.Body)
        {
            switch (Visit(command))
            {
                case null: continue;
                case Block { Shadow: false } block:
                {
                    if (_parent != null)
                    {
                        block.Parent = _parent.Id;
                        _parent.Next = block.Id;
                    }

                    _parent = block;
                    blocks.Add(block);
                    break;
                }
                case List<Block> stack:
                {
                    if (stack.Count == 0) break;

                    AttachStackToBlock(_parent, stack);
                    _parent = stack.Last();
                    blocks.AddRange(stack);
                    break;
                }
            }
        }

        _parent = lastParent;
        foreach (var block in blocks) Target.Blocks[block.Id] = block;
        return blocks;
    }

    public override object? VisitProgram(IrProgramNode node)
    {
        foreach (var blockNode in node.Blocks)
            Visit(blockNode);
        return null;
    }

    public override object? VisitFunction(IrFunctionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitEvent(IrEventNode node)
    {
        var block = new Block
        {
            TopLevel = true,
            Opcode = node.Type switch
            {
                "start" => Control.WhenFlagClicked,
                _ => throw new ArgumentOutOfRangeException()
            }
        };
        block.Id = GenerateBlockId(block.Opcode);

        var stack = VisitScope(node.Scope);
        AttachStackToBlock(block, stack);
        Target.Blocks[block.Id] = block;
        return block;
    }

    public override object? VisitCommandSequence(IrCommandSequenceNode node)
        => VisitScope(new Scope { Body = node.Commands.ToList() });

    public override object? VisitSetCommand(IrSetCommandNode node)
    {
        throw new Exception("This command is not supported by the ScratchScriptProjectEmitter");
    }

    public override object? VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitConstantExpression(IrConstantExpressionNode node)
        => node.Value.Value;

    public override object? VisitGlobalVariableExpression(IrGlobalVariableIdentifierExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitLocalVariableExpression(IrLocalVariableIdentifierExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitGlobalListExpression(IrGlobalListIdentifierExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitParenthesizedExpression(IrParenthesizedExpressionNode node)
        => Visit(node.Expression);

    public override object? VisitComplexExpression(IrComplexExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        throw new NotImplementedException();
    }
}
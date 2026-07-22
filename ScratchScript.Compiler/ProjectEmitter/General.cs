using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.ProjectEmitter.Blocks;
using ScratchScript.Compiler.ProjectEmitter.Models;

namespace ScratchScript.Compiler.ProjectEmitter;

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

        _parent = lastParent;
        foreach (var block in blocks) Target.Blocks[block.Id] = block;
        return blocks;
    }

    public override object? VisitTargetSpecificNode(ITargetSpecificNode node) => throw new NotImplementedException();

    public override object? VisitProgram(IrProgramNode node)
    {
        foreach (var blockNode in node.Functions)
            Visit(blockNode);
        foreach (var revisitNodes in _revisitFunctions)
            RevisitFunction(revisitNodes);
        foreach (var blockNode in node.Events)
            Visit(blockNode);
        return null;
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

    public override object? VisitRawBlock(IrBlockNode node) => VisitScope(node.Scope);

    // TODO: implement if the project emitter will ever need attributes
    public override object? VisitAttribute(IrAttributeNode node)
        => null;

    public override object? VisitNoOpCommand(IrNoOpCommandNode node) => null;

    public override object? VisitCommandSequence(IrCommandSequenceNode node) =>
        VisitScope(new Scope { Body = node.Commands.ToList() });

    public override object? VisitSetCommand(IrSetCommandNode node)
    {
        if (!ReservedNames.GlobalVariables.Contains(node.Variable))
            throw new Exception("This command is not supported by the ScratchScriptProjectEmitter");
        var value = Visit(node.Expression);
        if (value == null) throw new Exception("The 'value' expression in VisitSetCommand was null.");
        var block = new Block { Opcode = Data.SetVariableTo, Id = GenerateBlockId(Data.SetVariableTo) };
        block.Fields["VARIABLE"] = CreateField(node.Variable);
        block.Inputs["VALUE"] = value is Block valueBlock ? CreateInput(valueBlock, block) : CreateInput(value);
        return block;
    }

    public override object? VisitBreakCommand(IrBreakCommandNode node) => throw new NotImplementedException();

    public override object? VisitContinueCommand(IrContinueCommandNode node) => throw new NotImplementedException();

    public override object? VisitConstantExpression(IrConstantExpressionNode node) => node.Value.Value;

    public override object? VisitGlobalVariableIdentifierExpression(IrGlobalVariableIdentifierExpressionNode node)
    {
        var block = new Block { Opcode = Data.Variable, Id = GenerateBlockId(Data.Variable) };
        block.Fields["VARIABLE"] = CreateField(node.Name);
        Target.Blocks[block.Id] = block;
        return block;
    }

    public override object? VisitLocalVariableIdentifierExpression(IrLocalVariableIdentifierExpressionNode node) =>
        throw new NotImplementedException();

    public override object? VisitGlobalListIdentifierExpression(IrGlobalListIdentifierExpressionNode node) =>
        throw new NotImplementedException();

    public override object? VisitParenthesizedExpression(IrParenthesizedExpressionNode node) => Visit(node.Expression);

    public override object? VisitComplexExpression(IrComplexExpressionNode node) => throw new NotImplementedException();

    public override object? VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node) =>
        throw new NotImplementedException();

    public override object? VisitTernaryExpression(IrTernaryExpressionNode node) => throw new NotImplementedException();

    public override object? VisitStackPointerExpressionNode(IrStackPointerExpressionNode node) =>
        throw new NotImplementedException();
}
using System.Globalization;
using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.GeneratedVisitor;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public record ScratchIRVisitorSettings(string VisitorId, bool UseStack);

public partial class ScratchIRVisitor(ScratchIRVisitorSettings settings) : ScratchIRBaseVisitor<object?>
{
    public readonly Target Target = new();
    private Block? _parent;
    public ScratchIRVisitorSettings Settings { get; init; } = settings;

    public override object? VisitConstant(ScratchIRParser.ConstantContext context)
    {
        if (context.Number() is { } n)
            return decimal.Parse(n.GetText(), CultureInfo.InvariantCulture);
        if (context.String() is { } s)
            return s.GetText()[1..^1];
        if (context.Color() is { } c)
            return c.GetText()[1..];
        return null;
    }

    public override object VisitEventBlock(ScratchIRParser.EventBlockContext context)
    {
        // TODO: temporary implementation. handle more event types later
        var block = new Block
        {
            TopLevel = true,
            Opcode = context.Event().GetText() switch
            {
                "start" => Control.WhenFlagClicked,
                _ => throw new ArgumentOutOfRangeException()
            }
        };
        block.Id = GenerateBlockId(block.Opcode);

        var commands = VisitCommands(context.command());
        AttachStackToBlock(block, commands);

        Target.Blocks[block.Id] = block;
        return block;
    }

    private List<Block> VisitCommands(IEnumerable<ScratchIRParser.CommandContext> commands)
    {
        var lastParent = _parent;
        _parent = null;

        var blocks = new List<Block>();
        foreach (var command in commands)
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

    public override object? VisitDefineStatement(ScratchIRParser.DefineStatementContext context)
    {
        var name = context.Identifier().GetText();
        var type = context.DefineType().GetText();
        var id = $"_{Settings.VisitorId[..5]}_{name}";

        if (type == "var")
        {
            Target.Variables[id] = [name, ""];
            if (context.constant().Length != 0)
                Target.Variables[id][1] = VisitConstant(context.constant(0)) ??
                                          throw new Exception("Expected a non-null value for DefineStatement.");
        }
        else if (type == "list")
        {
            Target.Lists[id] = [name, context.constant().Select(Visit).OfType<object>().Select(obj => obj.ToString())];
        }

        return null;
    }
}
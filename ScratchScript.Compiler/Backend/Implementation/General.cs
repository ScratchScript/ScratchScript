using System.Globalization;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor(string Id): ScratchIRBaseVisitor<object?>
{
    public Target Target = new();
    private Block? _parent;
    
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
                "start" => "event_whenflagclicked",
                _ => throw new ArgumentOutOfRangeException()
            }
        };
        block.Id = GenerateBlockId(block.Opcode);

        var commands = VisitCommands(context.command());
        AttachStackToBlock(block, commands);
        UpdateBlocks(block);
        return block;
    }

    private List<Block> VisitCommands(IEnumerable<ScratchIRParser.CommandContext> commands)
    {
        var lastParent = _parent;
        var blocks = new List<Block>();
        _parent = null;

        foreach (var command in commands)
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
        UpdateBlocks(blocks.ToArray());
        return blocks;
    }
    
    private readonly Dictionary<string, int> _blockNameUsage = [];

    private string GenerateBlockId(string opcode)
    {
        // TODO: temporary implementation. may be changed later for space optimizations and similar stuff
        _blockNameUsage.TryAdd(opcode, 0);
        _blockNameUsage[opcode]++;
        return $"_{Id[..5]}_{opcode}_{_blockNameUsage[opcode]}";
    }

    private void AttachStackToBlock(Block? parent, IEnumerable<Block> stack)
    {
        var list = stack.ToList();
        
        if (list.Count == 0) return;
        if (parent == null) return;

        var first = list.First();
        first.Parent = parent.Id;
        parent.Next = first.Id;
    }
    
    private void UpdateBlocks(params Block[] blocks)
    {
        foreach (var block in blocks)
            Target.Blocks[block.Id] = block;
    }
}
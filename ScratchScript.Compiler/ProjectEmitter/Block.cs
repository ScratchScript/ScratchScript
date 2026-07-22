using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.ProjectEmitter.Models;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.ProjectEmitter;

public enum ScratchShadowType
{
    Shadow = 1,
    NoShadow = 2,
    ObscuredShadow = 3
}

public partial class ScratchScriptProjectEmitter
{
    private readonly Dictionary<string, int> _blockNameUsage = [];

    private string GenerateBlockId(string opcode)
    {
        // TODO: temporary implementation. may be changed later for space optimizations and similar stuff
        _blockNameUsage.TryAdd(opcode, 0);
        _blockNameUsage[opcode]++;
        return $"_{SourceHash[..5]}_{opcode}_{_blockNameUsage[opcode]}";
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

    private void AttachStackToBlock(Block? parent, IEnumerable<Block> stack, string to)
    {
        var list = stack.ToList();

        if (list.Count == 0) return;
        if (parent == null) return;

        var first = list.First();
        first.Parent = parent.Id;

        parent.Inputs[to] = CreateInput(first, parent);
    }

    private List<object> CreateInput(Block shadow, Block? parent)
    {
        if (string.IsNullOrEmpty(shadow.Id) || (parent != null && string.IsNullOrEmpty(parent.Id)))
            throw new Exception("Blocks passed to CreateInput didn't have ids assigned to them.");
        if (parent != null)
            shadow.Parent = parent.Id;
        // TODO: handle data_variable and data_list
        return [(int)ScratchShadowType.Shadow, shadow.Id];
    }

    private List<object> CreateInput(object obj)
    {
        var type = obj switch
        {
            int or double => ScratchType.Number,
            string => ScratchType.String,
            bool => ScratchType.Boolean,
            _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
        };
        return [(int)ScratchShadowType.Shadow, new List<object> { (int)type.Kind, obj }];
    }

    // TODO: handle argument & variable reporters
    private List<object> CreateField(object obj) => [obj, obj];

    public override object? VisitRawCommand(IrRawCommandNode node)
    {
        var block = new Block
        {
            Opcode = node.Opcode,
            TopLevel = false,
            Shadow = false,
            Id = GenerateBlockId(node.Opcode)
        };
        SetRawBlockProperties(ref block, node.Inputs, node.Fields);
        return block;
    }

    public override object? VisitShadowExpression(IrShadowExpressionNode node)
    {
        var block = new Block
        {
            Opcode = node.Opcode,
            TopLevel = false,
            Shadow = true,
            Id = GenerateBlockId(node.Opcode)
        };
        SetRawBlockProperties(ref block, node.Inputs, node.Fields);
        Target.Blocks[block.Id] = block;
        return block;
    }

    private void SetRawBlockProperties(ref Block block,
        Dictionary<string, IrExpressionNode> inputs, Dictionary<string, IrExpressionNode> fields)
    {
        foreach (var (name, expressionNode) in inputs)
        {
            var value = Visit(expressionNode);
            if (value == null) throw new Exception("An expression in SetRawBlockProperties was null.");
            block.Inputs[name] = value is Block inputBlock ? CreateInput(inputBlock, block) : CreateInput(value);
        }

        foreach (var (name, expressionNode) in fields)
        {
            var value = Visit(expressionNode);
            if (value == null) throw new Exception("An expression in SetRawBlockProperties was null.");
            block.Fields[name] = CreateField(value);
        }
    }
}
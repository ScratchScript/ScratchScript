using ScratchScript.Compiler.Models;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Backend.Implementation;

public enum ScratchShadowType
{
    Shadow = 1,
    NoShadow = 2,
    ObscuredShadow = 3
}

public partial class ScratchIRVisitor
{
    private readonly Dictionary<string, int> _blockNameUsage = [];

    private string GenerateBlockId(string opcode)
    {
        // TODO: temporary implementation. may be changed later for space optimizations and similar stuff
        _blockNameUsage.TryAdd(opcode, 0);
        _blockNameUsage[opcode]++;
        return $"_{Settings.VisitorId[..5]}_{opcode}_{_blockNameUsage[opcode]}";
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
            int => ScratchType.Number,
            decimal => ScratchType.Number,
            string => ScratchType.String,
            bool => ScratchType.Boolean,
            _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
        };
        return [(int)ScratchShadowType.Shadow, new List<object> { (int)type.Kind, obj }];
    }

    // TODO: handle argument & variable reporters
    private List<object> CreateField(object obj)
    {
        return [obj, obj];
    }

    public override object VisitRawCommand(ScratchIRParser.RawCommandContext context)
    {
        var opcode = context.Identifier().GetText();
        var block = new Block
        {
            Opcode = opcode,
            TopLevel = false,
            Shadow = false,
            Id = GenerateBlockId(opcode)
        };
        SetRawBlockProperties(ref block, context.callFunctionArgument());
        return block;
    }

    public override object VisitRawShadowExpression(ScratchIRParser.RawShadowExpressionContext context)
    {
        var opcode = context.Identifier().GetText();
        var block = new Block
        {
            Opcode = opcode,
            TopLevel = false,
            Shadow = true,
            Id = GenerateBlockId(opcode)
        };
        SetRawBlockProperties(ref block, context.callFunctionArgument());
        Target.Blocks[block.Id] = block;
        return block;
    }

    private void SetRawBlockProperties(ref Block block,
        IEnumerable<ScratchIRParser.CallFunctionArgumentContext> properties)
    {
        foreach (var property in properties)
        {
            var name = property.Identifier().GetText();
            var value = Visit(property.expression());
            if (value == null) throw new Exception("An expression in SetRawBlockProperties was null.");

            if (property.functionArgumentType().GetText().StartsWith('i'))
                block.Inputs[name] = value is Block inputBlock ? CreateInput(inputBlock, block) : CreateInput(value);
            else
                block.Fields[name] = CreateField(value);
        }
    }
}
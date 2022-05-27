namespace ScratchSharp.Blocks;

public class BlockBuilder
{
    private string _opcode;
    private string? _parent = null;
    private bool _topLevel = true;
    private int? _x = null;
    private int? _y = null;
    private bool _shadow;


    public BlockBuilder WithOpcode(string opcode)
    {
        _opcode = opcode;
        return this;
    }
    
    public BlockBuilder WithParent(string? parent)
    {
        _topLevel = string.IsNullOrEmpty(parent);
        _parent = parent;
        return this;
    }

    public BlockBuilder WithPosition(int x, int y)
    {
        _x = x;
        _y = y;
        return this;
    }

    public BlockBuilder IsShadow(bool shadow = true)
    {
        _shadow = shadow;
        return this;
    }
}
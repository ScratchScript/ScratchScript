using ScratchScript.Wrapper;

namespace ScratchSharp.Blocks;

public class BlockBuilder
{
    private string _opcode;
    private string? _parent = null;
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
    public BlockBuilder IsTopLevel(bool topLevel)
    {
        if(topLevel)
            _parent = null;
        return this;
    }

    public Block Build()
    {
        return new Block()
        {
            opcode = _opcode,
            parent = _parent,

            x = _x,
            y = _y,
            shadow = _shadow
        };
    }
}
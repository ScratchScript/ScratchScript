using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Backend.Representation;

public class IrShadowBuilder
{
    private readonly string _opcode;
    private readonly Dictionary<string, IrExpressionNode> _inputs = new();
    private readonly Dictionary<string, IrExpressionNode> _fields = new();

    private IrShadowBuilder(string opcode) => _opcode = opcode;

    public static IrShadowBuilder FromOpcode(string opcode) => new(opcode);

    public IrShadowBuilder WithInput(string name, IrExpressionNode value)
    {
        _inputs[name] = value;
        return this;
    }

    public IrShadowBuilder WithField(string name, IrExpressionNode value)
    {
        _fields[name] = value;
        return this;
    }
    
    public IrShadowBuilder WithField(string name, string value)
    {
        _fields[name] = new IrConstantExpressionNode(TypedValue.String(value));
        return this;
    }

    public IrRawCommandNode BuildCommand() => new(_opcode, _inputs, _fields);

    public IrShadowExpressionNode BuildExpression(ScratchType? expectedType = null) =>
        new(_opcode, _inputs, _fields, expectedType);
}
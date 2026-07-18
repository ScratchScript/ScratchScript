using ScratchScript.Compiler.TypeChecker;
using ScratchType = ScratchScript.Compiler.TypeChecker.ScratchType;

namespace ScratchScript.Compiler.AST.Representation;

public class IrShadowBuilder
{
    private readonly Dictionary<string, IrExpressionNode> _fields = new();
    private readonly Dictionary<string, IrExpressionNode> _inputs = new();
    private readonly string _opcode;

    private IrShadowBuilder(string opcode)
    {
        _opcode = opcode;
    }

    public static IrShadowBuilder FromOpcode(string opcode)
    {
        return new IrShadowBuilder(opcode);
    }

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

    public IrRawCommandNode BuildCommand()
    {
        return new IrRawCommandNode(_opcode, _inputs, _fields);
    }

    public IrShadowExpressionNode BuildExpression(ScratchType? expectedType = null)
    {
        return new IrShadowExpressionNode(_opcode, _inputs, _fields, expectedType);
    }
}
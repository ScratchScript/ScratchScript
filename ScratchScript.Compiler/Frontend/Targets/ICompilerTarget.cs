namespace ScratchScript.Compiler.Frontend.Targets;

public interface ICompilerTarget
{
    public IAttributeHandler Attribute { get; }
    public IBinaryHandler Binary { get; }
    public IConditionalHandler Conditional { get; }
    public IDataHandler Data { get; }
    public IEnumHandler Enum { get; }
    public IFunctionHandler Function { get; }
    public IUnaryHandler Unary { get; }
}
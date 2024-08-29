namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3CompilerTarget(char commandSeparator) : ICompilerTarget
{
    public IAttributeHandler Attribute { get; } = new Scratch3AttributeHandler();
    public IBinaryHandler Binary { get; } = new Scratch3BinaryHandler(commandSeparator);
    public IConditionalHandler Conditional { get; } = new Scratch3ConditionalHandler();
    public IDataHandler Data { get; } = new Scratch3DataHandler();
    public IEnumHandler Enum { get; } = new Scratch3EnumHandler();
    public IFunctionHandler Function { get; } = new Scratch3FunctionHandler();
    public IUnaryHandler Unary { get; } = new Scratch3UnaryHandler();
}
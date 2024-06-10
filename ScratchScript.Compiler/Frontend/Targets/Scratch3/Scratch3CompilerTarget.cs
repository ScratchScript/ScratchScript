namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3CompilerTarget(char commandSeparator) : ICompilerTarget
{
    public IAttributeHandler Attribute { get; init; } = new Scratch3AttributeHandler();
    public IBinaryHandler Binary { get; init; } = new Scratch3BinaryHandler(commandSeparator);
    public IConditionalHandler Conditional { get; init; } = new Scratch3ConditionalHandler();
    public IDataHandler Data { get; init; } = new Scratch3DataHandler();
    public IEnumHandler Enum { get; init; } = new Scratch3EnumHandler();
    public IFunctionHandler Function { get; init; } = new Scratch3FunctionHandler();
    public IUnaryHandler Unary { get; init; } = new Scratch3UnaryHandler();
}
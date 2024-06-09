using ScratchScript.Compiler.Frontend.Targets;

namespace ScratchScript.Compiler.Types;

public enum IdentifierType
{
    Variable,
    FunctionArgument,
    CustomType
}

public record TypedValue(object? Value, ScratchType Type);

public record TypeDeclarationValue(ScratchType Type) : TypedValue(null, Type);

public record EnumEntryValue(string Name, object? Value, ScratchType Type) : TypedValue(Value, Type);

public record ExpressionValue(
    object? Value,
    ScratchType Type,
    IEnumerable<string>? Dependencies = null,
    IEnumerable<string>? Cleanup = null,
    bool ContainsIntermediateRepresentation = true)
    : TypedValue(Value, Type);

public record IdentifierExpressionValue(
    IdentifierType IdentifierType,
    string Identifier,
    IScope? RelatedScope,
    object? Value,
    ScratchType Type) : ExpressionValue(Value, Type);

public record StatementValue(
    IEnumerable<string> Commands,
    IEnumerable<string>? Dependencies = null,
    IEnumerable<string>? Cleanup = null) : TypedValue(Commands, ScratchType.Unknown);

public record ScopeValue(IScope Scope) : TypedValue(null, ScratchType.Unknown);

public record GenericValue<T>(T Value) : TypedValue(Value, ScratchType.Unknown)
{
    public new T Value => (T)(base.Value ?? throw new Exception());
}
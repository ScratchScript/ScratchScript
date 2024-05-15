using ScratchScript.Compiler.Frontend.Targets;

namespace ScratchScript.Compiler.Types;

public record TypedValue(object? Value, ScratchType Type);

public record TypeDeclarationValue(ScratchType Type) : TypedValue(null, Type);

public record EnumEntryValue(string Name, object? Value, ScratchType Type) : TypedValue(Value, Type);

public record ExpressionValue(object? Value, ScratchType Type, string Dependencies = "", string Cleanup = "")
    : TypedValue(Value, Type);

public record ScopeValue(Scope Scope) : TypedValue(null, ScratchType.Unknown);

public record GenericValue<T>(T Value) : TypedValue(Value, ScratchType.Unknown)
{
    public new T Value => (T)(base.Value ?? throw new Exception());
}
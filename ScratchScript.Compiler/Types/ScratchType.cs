namespace ScratchScript.Compiler.Types;

public enum ScratchTypeKind
{
    Unknown = 0,
    Number = 4,
    Color = 9,
    String = 10,
    Variable = 12,
    List = 13,
    Boolean = 15,

    // these should not be encoded to blocks
    Any,
    Enum
}

public class ScratchType(ScratchTypeKind kind, ScratchType? childType = null, ScratchType? parentType = null)
    : IEquatable<ScratchType>
{
    public static readonly ScratchType Unknown = new(ScratchTypeKind.Unknown);
    public static readonly ScratchType Number = new(ScratchTypeKind.Number);
    public static readonly ScratchType String = new(ScratchTypeKind.String);
    public static readonly ScratchType Boolean = new(ScratchTypeKind.Boolean);
    public static readonly ScratchType Color = new(ScratchTypeKind.Color);

    // custom types for the compiler
    public static readonly ScratchType Any = new(ScratchTypeKind.Any);

    public ScratchTypeKind Kind { get; set; } = kind;
    public ScratchType? ParentType { get; set; } = parentType;
    public ScratchType? ChildType { get; set; } = childType;

    public bool Equals(ScratchType? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Kind == other.Kind && ChildType == other.ChildType && ParentType == other.ParentType;
    }

    public static ScratchType List(ScratchType innerType)
    {
        var type = new ScratchType(ScratchTypeKind.List);
        innerType.ParentType = type;
        type.ChildType = innerType;
        return type;
    }

    public override string ToString()
    {
        return $"{Kind.ToString().ToLowerInvariant()}{(ChildType is not null ? $"<{ChildType}>" : "")}";
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ScratchType);
    }

    public static bool operator ==(ScratchType? first, ScratchType? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    public static bool operator !=(ScratchType? first, ScratchType? second)
    {
        return !(first == second);
    }


    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
}

public class EnumScratchType(string name, ScratchType valueType, Dictionary<string, EnumEntryValue> values)
    : ScratchType(ScratchTypeKind.Enum, valueType)
{
    public string Name { get; } = name;
    public Dictionary<string, EnumEntryValue> Values { get; } = values;
}
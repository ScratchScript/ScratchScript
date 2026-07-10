using System.Text.RegularExpressions;
using ScratchScript.Compiler.Backend.Representation;

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
    Enum,
    Void,
    Object
}

public partial class ScratchType(ScratchTypeKind kind, ScratchType? childType = null, ScratchType? parentType = null)
    : IEquatable<ScratchType>
{
    public static readonly ScratchType Unknown = new(ScratchTypeKind.Unknown);
    public static readonly ScratchType Number = new(ScratchTypeKind.Number);
    public static readonly ScratchType String = new(ScratchTypeKind.String);
    public static readonly ScratchType Boolean = new(ScratchTypeKind.Boolean);
    public static readonly ScratchType Color = new(ScratchTypeKind.Color);

    // custom types for the compiler
    public static readonly ScratchType Any = new(ScratchTypeKind.Any);
    public static readonly ScratchType Void = new(ScratchTypeKind.Void);
    public static readonly ScratchType Object = new(ScratchTypeKind.Object);

    public ScratchTypeKind Kind { get; set; } = kind;
    public ScratchType? ParentType { get; set; } = parentType;
    public ScratchType? ChildType { get; set; } = childType;

    public bool Equals(ScratchType? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Kind == ScratchTypeKind.Any || other.Kind == ScratchTypeKind.Any) return true;
        return Kind == other.Kind && ChildType == other.ChildType && ParentType == other.ParentType;
    }

    public static ScratchType List(ScratchType innerType)
    {
        var type = new ScratchType(ScratchTypeKind.List);
        innerType.ParentType = type;
        type.ChildType = innerType;
        return type;
    }

    public static ScratchType? FromString(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var matches = TypeRegex().Matches(str);
        if (matches.Count != 1) throw new Exception($"Invalid type `{str}`: must match regex exactly once");
        var (parent, child) = (matches.First().Groups["parent"].Value, matches.First().Groups["child"].Value);

        if (string.IsNullOrEmpty(child))
            return parent switch
            {
                "number" => Number,
                "string" => String,
                "boolean" => Boolean,
                "color" => Color,
                "any" => Any,
                "object" => Object,
                "list" => new ScratchType(ScratchTypeKind.List),
                _ => null
            };

        var (parentType, childType) = (FromString(parent), FromString(child));
        if (parentType == null) throw new Exception($"Invalid type `{str}`: parentType is null");
        if (parentType.ChildType != null)
            throw new Exception($"Invalid type `{str}`: childType of parentType must be null");

        var type = new ScratchType(parentType.Kind);
        if (childType != null) childType.ParentType = type;
        type.ChildType = childType;
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

    [GeneratedRegex(@"(?<parent>[\w\-$@#]+)(?:<(?<child>.*)>)?")]
    private static partial Regex TypeRegex();
}

public record TypedValue(object? Value, ScratchType Type)
{
    public static TypedValue String(string value) => new(value, ScratchType.String);
    public static TypedValue Color(string value) => new(value, ScratchType.Color);
    public static TypedValue Number(double value) => new(value, ScratchType.Number);
    public static TypedValue Boolean(bool value) => new(value, ScratchType.Boolean);
    public static TypedValue Object(Dictionary<string, IrExpressionNode> values) => new(values, ScratchType.Object);
}
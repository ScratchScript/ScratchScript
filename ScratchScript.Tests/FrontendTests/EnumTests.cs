using FluentAssertions;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Types;
using ScratchScript.Tests.Utils;

namespace ScratchScript.Tests.FrontendTests;

public class EnumTests
{
    [Fact]
    public void EmptyEnum()
    {
        var (visitor, _) = VisitorUtils.Run("enum TestEnum {}");

        visitor.Success.Should().BeTrue();
        visitor.Exports.Enums.Values.Should().ContainSingle("because an empty enum was created").Which.Should()
            .BeEquivalentTo(new EnumScratchType("TestEnum", ScratchType.Number,
                new Dictionary<string, EnumEntryValue>()), "because that's the C# interpretation of the enum");
    }

    [Fact]
    public void NumericEnumWithInitializers()
    {
        var (visitor, _) = VisitorUtils.Run("enum TestEnum { first = 1, second = 2, third = 3 }");

        visitor.Success.Should().BeTrue();
        visitor.Exports.Enums.Values.Should().ContainSingle("because a single enum was created");
        visitor.Exports.Enums.Values.First().Values.Should().Contain(new List<KeyValuePair<string, EnumEntryValue>>
        {
            new("first", new EnumEntryValue("first", (decimal)1, ScratchType.Number)),
            new("second", new EnumEntryValue("second", (decimal)2, ScratchType.Number)),
            new("third", new EnumEntryValue("third", (decimal)3, ScratchType.Number))
        });
    }

    [Fact]
    public void NumericEnumWithDefaultValues()
    {
        var (visitor, _) = VisitorUtils.Run("enum TestEnum { first, second, third }");
        visitor.Success.Should().BeTrue();
        visitor.Exports.Enums.Values.Should().ContainSingle("because a single enum was created");
        visitor.Exports.Enums.Values.First().ChildType.Should()
            .Be(ScratchType.Number, "because the default enum type is number");
        visitor.Exports.Enums.Values.First().Values.Should().Contain(new List<KeyValuePair<string, EnumEntryValue>>
        {
            new("first", new EnumEntryValue("first", (decimal)0, ScratchType.Number)),
            new("second", new EnumEntryValue("second", (decimal)1, ScratchType.Number)),
            new("third", new EnumEntryValue("third", (decimal)2, ScratchType.Number))
        });
    }

    [Fact]
    public void StringEnumWithInitializers()
    {
        var (visitor, _) = VisitorUtils.Run("enum TestEnum { first = 'hi', second = 'hey', third = 'hello' }");
        visitor.Success.Should().BeTrue();
        visitor.Exports.Enums.Values.Should().ContainSingle("because a single enum was created");
        visitor.Exports.Enums.Values.First().Values.Should().Contain(new List<KeyValuePair<string, EnumEntryValue>>
        {
            new("first", new EnumEntryValue("first", "'hi'", ScratchType.String)),
            new("second", new EnumEntryValue("second", "'hey'", ScratchType.String)),
            new("third", new EnumEntryValue("third", "'hello'", ScratchType.String))
        });
    }

    [Fact]
    public void StringEnumWithMissingValuesShouldError()
    {
        var (visitor, messages) = VisitorUtils.Run("enum TestEnum {first, second, third = 'hey'}");
        visitor.Success.Should().BeFalse();
        messages.Should().Contain(message =>
            message.Kind == DiagnosticMessageKind.Error &&
            message.Code == (int)ScratchScriptError.NonNumericEntryMustSpecifyAllValues);
    }

    [Fact]
    public void EnumWithConflictingTypesShouldError()
    {
        var (visitor, messages) = VisitorUtils.Run("enum TestEnum { first = 1, second = 'h', third = 2 }");
        visitor.Success.Should().BeFalse();
        messages.Should().Contain(message =>
            message.Kind == DiagnosticMessageKind.Error &&
            message.Code == (int)ScratchScriptError.TypeMismatch);
    }

    [Fact]
    public void EnumWithDuplicateEntriesShouldError()
    {
        var (visitor, messages) = VisitorUtils.Run("enum TestEnum { first, second, first }");
        visitor.Success.Should().BeFalse();
        messages.Should().Contain(message =>
            message.Kind == DiagnosticMessageKind.Error &&
            message.Code == (int)ScratchScriptError.EnumEntryAlreadyDeclared);
    }
}
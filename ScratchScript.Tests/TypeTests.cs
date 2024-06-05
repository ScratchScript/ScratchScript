using FluentAssertions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Tests;

public class TypeTests
{
    [Theory]
    [MemberData(nameof(SerializationTestData))]
    public void CorrectSerialization(ScratchType type, string expected)
    {
        type.ToString().Should().Be(expected);
    }

    public static IEnumerable<object[]> SerializationTestData()
    {
        yield return [ScratchType.Number, "number"];
        yield return [ScratchType.List(ScratchType.Number), "list<number>"];
        yield return [ScratchType.List(ScratchType.List(ScratchType.Number)), "list<list<number>>"];
        yield return [new EnumScratchType("TestEnum", ScratchType.Number, []), "enum<number>"];
    }
}
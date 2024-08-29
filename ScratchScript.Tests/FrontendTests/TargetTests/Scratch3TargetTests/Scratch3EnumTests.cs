using FluentAssertions;
using ScratchScript.Compiler.Frontend.Targets.Scratch3;
using ScratchScript.Tests.Utils;

namespace ScratchScript.Tests.FrontendTests.TargetTests.Scratch3TargetTests;

public class Scratch3EnumTests
{
    [Fact]
    public void CorrectSerialization()
    {
        var (visitor, _) = VisitorUtils.Run("enum TestEnum { first = 1, second = 2, third = 3 }");
        visitor.Success.Should().BeTrue();

        var expected = string.Join(visitor.Settings.CommandSeparator, [
            $"define list {Scratch3EnumHandler.EnumIdsList} \"TestEnum_first\" \"TestEnum_second\" \"TestEnum_third\"",
            $"define list {Scratch3EnumHandler.EnumPropertiesList} \"first\" \"second\" \"third\"",
            $"define list {Scratch3EnumHandler.EnumValuesList} 1 2 3"
        ]);

        visitor.Output.Should().Contain(expected);
    }
}
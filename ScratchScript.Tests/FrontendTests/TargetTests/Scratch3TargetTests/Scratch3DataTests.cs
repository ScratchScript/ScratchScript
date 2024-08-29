using FluentAssertions;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Frontend.Targets.Scratch3;
using ScratchScript.Tests.Utils;

namespace ScratchScript.Tests.FrontendTests.TargetTests.Scratch3TargetTests;

public class Scratch3DataTests
{
    [Fact]
    public void CorrectSerialization()
    {
        var (visitor, _) = VisitorUtils.Run("on start { let a = 1; }");
        visitor.Success.Should().BeTrue();

        var variableId = visitor.Target.Data.GenerateVariableId(0, visitor.Id, "a").Surround('"');
        var scopeContent =
            (visitor.Exports.Events["start"] as Scratch3Scope)!.ToString(visitor.Settings.CommandSeparator);

        scopeContent.Should().Be(string.Join(visitor.Settings.CommandSeparator, [
            "on start",
            Scratch3Helper.Push(Scratch3Helper.VariableNamesList, variableId),
            Scratch3Helper.Push(Scratch3Helper.VariableValuesList, 1),
            Scratch3Helper.PopAt(Scratch3Helper.VariableValuesList,
                Scratch3Helper.IndexOf(Scratch3Helper.VariableNamesList, variableId)),
            Scratch3Helper.PopAt(Scratch3Helper.VariableNamesList,
                Scratch3Helper.IndexOf(Scratch3Helper.VariableNamesList, variableId)),
            "end"
        ]));
    }

    [Fact]
    public void CorrectGetter()
    {
        // disable compile-time evaluation because b will just become 3
        var (visitor, _) = VisitorUtils.Run("on start { let a = 1; let b = a + 2; }",
            new ScratchScriptVisitorSettings('\n', false));
        visitor.Success.Should().BeTrue();

        var firstId = visitor.Target.Data.GenerateVariableId(0, visitor.Id, "a");
        var secondId = visitor.Target.Data.GenerateVariableId(0, visitor.Id, "b");

        visitor.Exports.Events["start"].Content.Should().Contain(Scratch3Helper.Push(Scratch3Helper.VariableNamesList,
            secondId.Surround('"')));
        visitor.Exports.Events["start"].Content.Should().Contain(Scratch3Helper.Push(Scratch3Helper.VariableValuesList,
            $"+ {Scratch3Helper.GetVariableValue(firstId)} 2"));
    }
}
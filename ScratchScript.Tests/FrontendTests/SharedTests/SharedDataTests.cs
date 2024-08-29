using FluentAssertions;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;
using ScratchScript.Tests.Utils;

namespace ScratchScript.Tests.FrontendTests.SharedTests;

public class SharedDataTests
{
    [Fact]
    public void CorrectDefinition()
    {
        var (visitor, _) = VisitorUtils.Run("on start { let a = 1; }");
        visitor.Success.Should().BeTrue();

        var variableId = visitor.Target.Data.GenerateVariableId(0, visitor.Id, "a");
        var expectedValue =
            new ExpressionValue((double)1, ScratchType.Number, ContainsIntermediateRepresentation: false);

        visitor.Exports.Events["start"].Variables.Should().Contain("a",
            new ScratchScriptVariable("a", variableId, ScratchType.Number, expectedValue),
            "because that's how the variable was defined");
    }

    [Fact]
    public void UnknownIdentifierShouldError()
    {
        var (visitor, messages) = VisitorUtils.Run("on start { let a = b + 1; }");
        visitor.Success.Should().BeFalse();
        messages.Should().Contain(message =>
            message.Kind == DiagnosticMessageKind.Error && message.Code == (int)ScratchScriptError.UnknownIdentifier);
    }
}
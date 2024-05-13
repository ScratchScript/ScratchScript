using System.Globalization;
using FluentAssertions;
using ScratchScript.Compiler.Diagnostics;

namespace ScratchScript.Tests;

public class DiagnosticTests
{
    [Theory]
    [InlineData(DiagnosticMessageKind.Error, int.MaxValue)]
    [InlineData(DiagnosticMessageKind.Warning, int.MaxValue)]
    [InlineData(DiagnosticMessageKind.Note, int.MaxValue)]
    public void ThrowOnInvalidCode(DiagnosticMessageKind kind, int code)
    {
        var action = () => DiagnosticMessage.FromCode(kind, code, []);
        action.Should().Throw<Exception>("because the code is invalid");
    }

    [Theory]
    [InlineData(DiagnosticMessageKind.Error)]
    [InlineData(DiagnosticMessageKind.Warning)]
    [InlineData(DiagnosticMessageKind.Note)]
    public void CorrectResourceAccess(DiagnosticMessageKind kind)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        DiagnosticMessage.FromCode(kind, 0, []).Message.Should().Be("Reserved");
    }

    [Fact]
    public void CorrectEventInvocation()
    {
        var reporter = new DiagnosticReporter();
        using var monitor = reporter.Monitor();

        reporter.Note(0, []);
        monitor.Should().Raise("Reported").WithArgs<DiagnosticMessage>(message =>
            message.Code == 0 && message.Kind == DiagnosticMessageKind.Note && message.Message == "Reserved");
    }
}
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Resources.Messages;

namespace ScratchScript.Compiler.Diagnostics;

public enum DiagnosticMessageKind
{
    Warning,
    Error,
    Note
}

public static class DiagnosticMessageKindExtensions
{
    public static string ToColor(this DiagnosticMessageKind kind)
    {
        return kind switch
        {
            DiagnosticMessageKind.Warning => "yellow",
            DiagnosticMessageKind.Error => "red",
            DiagnosticMessageKind.Note => "grey",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}

public record SourceCodeDiagnosticMessage(
    DiagnosticMessageKind Kind,
    int Code,
    string SourceText,
    string SourceName,
    string ConflictingText,
    string Message,
    Interval SourceInterval,
    int ConflictingColumn,
    int ConflictingLine
) : DiagnosticMessage(Kind, Code, Message)
{
    public static SourceCodeDiagnosticMessage FromRuleContext(DiagnosticMessageKind kind, int code,
        ParserRuleContext start,
        ParserRuleContext conflicting, object[] data)
    {
        var sourceStream = start.Start.InputStream;
        var line = start.GetParent<ScratchScriptParser.TopLevelStatementContext>();
        if (line == null) throw new Exception("Failed to get the statement's parent TopLevelStatementContext.");

        var sourceText = sourceStream.GetText(new Interval(line.Start.StartIndex, line.Stop.StopIndex));
        var conflictingText =
            sourceStream.GetText(new Interval(conflicting.Start.StartIndex, conflicting.Stop.StopIndex));

        return new SourceCodeDiagnosticMessage(
            kind,
            code,
            Message: GetDiagnosticMessage(kind, code, data),
            ConflictingText: conflictingText,
            SourceText: sourceText,
            SourceName: sourceStream.SourceName,
            SourceInterval: new Interval(line.Start.Line, line.Stop.Line),
            ConflictingColumn: conflicting.Start.Column,
            ConflictingLine: conflicting.Start.Line);
    }

    public static SourceCodeDiagnosticMessage FromTerminalNode(DiagnosticMessageKind kind, int code,
        ParserRuleContext start,
        ITerminalNode conflicting, object[] data)
    {
        var sourceStream = start.Start.InputStream;
        var line = start.GetParent<ScratchScriptParser.TopLevelStatementContext>();
        if (line == null) throw new Exception("Failed to get the statement's parent TopLevelStatementContext.");

        var sourceText = sourceStream.GetText(new Interval(line.Start.StartIndex, line.Stop.StopIndex));
        var conflictingText =
            sourceStream.GetText(new Interval(conflicting.Symbol.StartIndex, conflicting.Symbol.StopIndex));

        return new SourceCodeDiagnosticMessage(
            kind,
            code,
            Message: GetDiagnosticMessage(kind, code, data),
            ConflictingText: conflictingText,
            SourceText: sourceText,
            SourceName: sourceStream.SourceName,
            SourceInterval: new Interval(line.Start.Line, line.Stop.Line),
            ConflictingColumn: conflicting.Symbol.Column,
            ConflictingLine: conflicting.Symbol.Line);
    }
}

public record DiagnosticMessage(
    DiagnosticMessageKind Kind,
    int Code,
    string Message
)
{
    public static DiagnosticMessage FromCode(DiagnosticMessageKind kind, int code, object[] data)
    {
        return new DiagnosticMessage(kind, code, GetDiagnosticMessage(kind, code, data));
    }

    protected static string GetDiagnosticMessage(DiagnosticMessageKind kind, int code, object[] data)
    {
        var unformatted = kind switch
        {
            DiagnosticMessageKind.Warning => CompilerWarnings.ResourceManager.GetString($"W{code}"),
            DiagnosticMessageKind.Error => CompilerErrors.ResourceManager.GetString($"E{code}"),
            DiagnosticMessageKind.Note => CompilerNotes.ResourceManager.GetString($"N{code}"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
        if (unformatted == null) throw new Exception($"Unknown code {code} for diagnostic type {kind}.");
        return string.Format(unformatted, data);
    }
}
using System.Text;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Resources.Messages;

namespace ScratchScript.Compiler.Diagnostics;

public abstract class DiagnosticMessageFormatter
{
    public abstract string Format(DiagnosticMessage message);
}

public class PlainDiagnosticMessageFormatter : DiagnosticMessageFormatter
{
    public override string Format(DiagnosticMessage message)
    {
        var sb = new StringBuilder();
        var longKindName = message.Kind.ToString().ToLowerInvariant();
        var shortKindName = message.Kind.ToString()[0];

        if (message is not SourceCodeDiagnosticMessage sourceCodeMessage)
        {
            // message
            sb.Append($"{longKindName}[{shortKindName}{message.Code}]");
            sb.AppendLine($": {message.Message}");
            return sb.ToString();
        }

        var start = sourceCodeMessage.SourceInterval.a;
        var end = sourceCodeMessage.SourceInterval.b;
        var padding = Math.Max(start, end).ToString().Length;
        var underline = new string(' ', Math.Max(sourceCodeMessage.ConflictingColumn, 0)) +
                        new string('^', sourceCodeMessage.ConflictingText.Length) +
                        " " + CompilerMessages.Here;

        // message
        sb.Append($"{longKindName}[{shortKindName}{message.Code}]");
        sb.AppendLine($": {sourceCodeMessage.Message}");

        // location
        sb.Append(new string(' ', padding));
        sb.Append("--> ");
        sb.AppendLine(
            $"{sourceCodeMessage.SourceName}:{sourceCodeMessage.ConflictingLine}:{sourceCodeMessage.ConflictingColumn + 1}");

        // source
        var sourceLines = sourceCodeMessage.SourceText.Split(["\r\n", "\n"], StringSplitOptions.None);
        for (var idx = start; idx <= end; idx++)
        {
            if (Math.Abs(sourceCodeMessage.ConflictingLine - idx) > 1) continue;

            sb.Append($"{idx}{new string(' ', Math.Max(padding - idx.ToString().Length + 1, 0))}| ");
            sb.AppendLine(sourceLines[idx - start]);

            if (idx != sourceCodeMessage.ConflictingLine) continue;

            sb.AppendLine($"{new string(' ', padding + 1)}|{underline}");
        }

        return sb.ToString();
    }
}

public class ColorDiagnosticMessageFormatter : DiagnosticMessageFormatter
{
    public override string Format(DiagnosticMessage message)
    {
        var sb = new StringBuilder();
        var longKindName = message.Kind.ToString().ToLowerInvariant();
        var shortKindName = message.Kind.ToString()[0];
        var kindColor = message.Kind.ToColor();

        if (message is not SourceCodeDiagnosticMessage sourceCodeMessage)
        {
            // message
            sb.Append($"[{kindColor}]{longKindName}[[{shortKindName}{message.Code}]][/]");
            sb.AppendLine($": {message.Message.ToAnsiConsoleCompatible()}");
            return sb.ToString();
        }

        var start = sourceCodeMessage.SourceInterval.a;
        var end = sourceCodeMessage.SourceInterval.b;
        var padding = Math.Max(start, end).ToString().Length;
        var underline = new string(' ', Math.Max(sourceCodeMessage.ConflictingColumn, 0)) +
                        new string('^', sourceCodeMessage.ConflictingText.Length) +
                        " " + CompilerMessages.Here;

        // message
        sb.Append($"[{sourceCodeMessage.Kind.ToColor()}]{longKindName}[[{shortKindName}{message.Code}]][/]");
        sb.AppendLine($": {sourceCodeMessage.Message.ToAnsiConsoleCompatible()}");

        // location
        sb.Append(new string(' ', padding));
        sb.Append("[grey]-->[/] ");
        sb.AppendLine(
            $"{sourceCodeMessage.SourceName}:{sourceCodeMessage.ConflictingLine}:{sourceCodeMessage.ConflictingColumn + 1}");

        // source
        var sourceLines = sourceCodeMessage.SourceText.Split(["\r\n", "\n"], StringSplitOptions.None);
        for (var idx = start; idx <= end; idx++)
        {
            if (Math.Abs(sourceCodeMessage.ConflictingLine - idx) > 1) continue;

            sb.Append($"[grey]{idx}{new string(' ', Math.Max(padding - idx.ToString().Length + 1, 0))}|[/] ");
            sb.AppendLine(sourceLines[idx - start].ToAnsiConsoleCompatible());

            if (idx != sourceCodeMessage.ConflictingLine) continue;

            sb.AppendLine($"[grey]{new string(' ', padding + 1)}|[/] [{kindColor}]{underline}[/]");
        }

        return sb.ToString();
    }
}
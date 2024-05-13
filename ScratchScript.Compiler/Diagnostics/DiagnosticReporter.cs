using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ScratchScript.Compiler.Diagnostics;

public class DiagnosticReporter
{
    public event Action<DiagnosticMessage> Reported;

    public void Error(int code, ParserRuleContext start, ParserRuleContext conflicting, params object[] data)
    {
        Reported.Invoke(
            SourceCodeDiagnosticMessage.FromRuleContext(DiagnosticMessageKind.Error, code, start, conflicting, data));
    }

    public void Error(int code, ParserRuleContext start, ITerminalNode conflicting, params object[] data)
    {
        Reported.Invoke(
            SourceCodeDiagnosticMessage.FromTerminalNode(DiagnosticMessageKind.Error, code, start, conflicting, data));
    }

    public void Warning(int code, ParserRuleContext start, ParserRuleContext conflicting, params object[] data)
    {
        Reported.Invoke(
            SourceCodeDiagnosticMessage.FromRuleContext(DiagnosticMessageKind.Warning, code, start, conflicting, data));
    }

    public void Warning(int code, ParserRuleContext start, ITerminalNode conflicting, params object[] data)
    {
        Reported.Invoke(
            SourceCodeDiagnosticMessage.FromTerminalNode(DiagnosticMessageKind.Warning, code, start, conflicting,
                data));
    }

    public void Note(int code, ParserRuleContext start, ParserRuleContext conflicting, params object[] data)
    {
        Reported.Invoke(
            SourceCodeDiagnosticMessage.FromRuleContext(DiagnosticMessageKind.Note, code, start, conflicting, data));
    }

    public void Note(int code, ParserRuleContext start, ITerminalNode conflicting, params object[] data)
    {
        Reported.Invoke(
            SourceCodeDiagnosticMessage.FromTerminalNode(DiagnosticMessageKind.Note, code, start, conflicting, data));
    }

    public void Note(int code, params object[] data)
    {
        Reported.Invoke(DiagnosticMessage.FromCode(DiagnosticMessageKind.Note, code, data));
    }
}
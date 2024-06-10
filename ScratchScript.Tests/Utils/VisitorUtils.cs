using Antlr4.Runtime;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Frontend.Targets.Scratch3;

namespace ScratchScript.Tests.Utils;

public abstract class VisitorUtils
{
    public static (ScratchScriptVisitor, List<DiagnosticMessage>) Run(string source,
        ScratchScriptVisitorSettings? settings = null, ICompilerTarget? target = null)
    {
        var inputStream = new AntlrInputStream(source);
        var lexer = new ScratchScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ScratchScriptParser(tokenStream);
        var messages = new List<DiagnosticMessage>();

        var visitor = new ScratchScriptVisitor(source);
        visitor.Settings = settings ?? new ScratchScriptVisitorSettings('\n');
        visitor.Target = target ?? new Scratch3CompilerTarget(visitor.Settings.CommandSeparator);
        visitor.DiagnosticReporter.Reported += message => messages.Add(message);
        visitor.VisitProgram(parser.program());

        return (visitor, messages);
    }
}
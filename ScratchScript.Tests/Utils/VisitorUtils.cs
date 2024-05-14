using Antlr4.Runtime;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Implementation;

namespace ScratchScript.Tests.Utils;

public class VisitorUtils
{
    public static (ScratchScriptVisitor, List<DiagnosticMessage>) Run(string source)
    {
        var inputStream = new AntlrInputStream(source);
        var lexer = new ScratchScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ScratchScriptParser(tokenStream);
        var messages = new List<DiagnosticMessage>();

        var visitor = new ScratchScriptVisitor(source);
        visitor.DiagnosticReporter.Reported += message => messages.Add(message);
        visitor.VisitProgram(parser.program());

        return (visitor, messages);
    }
}
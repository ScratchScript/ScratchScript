﻿using Antlr4.Runtime;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Implementation;
using Spectre.Console;
using ScratchScriptVisitor = ScratchScript.Compiler.Frontend.Implementation.ScratchScriptVisitor;


var source = """
             on start {
                let a = 1;
                a = 2;
             }
             """;
var inputStream = new AntlrInputStream(source);
var lexer = new ScratchScriptLexer(inputStream);
var tokenStream = new CommonTokenStream(lexer);
var parser = new ScratchScriptParser(tokenStream);
var visitor = new ScratchScriptVisitor(source);
visitor.DiagnosticReporter.Reported +=
    message => AnsiConsole.MarkupLine(new ColorDiagnosticMessageFormatter().Format(message));
visitor.Settings = new ScratchScriptVisitorSettings(CommandSeparator: '\n');
visitor.Visit(parser.program());
Console.WriteLine(visitor.Output);
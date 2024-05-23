﻿using Antlr4.Runtime;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Implementation;
using Spectre.Console;
using ScratchScriptVisitor = ScratchScript.Compiler.Frontend.Implementation.ScratchScriptVisitor;


const string source = """
                      function sum(x: number, y: number) {
                        x = x + 2;
                        return x + y * 2;
                      }

                      on start {
                        let a = 0;
                        if(1 + 2 == 3) {
                            a = sum(1, 2);
                        }
                        else {
                            a = sum(0, 0);
                        }
                      }
                      """;
var inputStream = new AntlrInputStream(source);
var lexer = new ScratchScriptLexer(inputStream);
var tokenStream = new CommonTokenStream(lexer);
var parser = new ScratchScriptParser(tokenStream);
var visitor = new ScratchScriptVisitor(source);
visitor.DiagnosticReporter.Reported +=
    message => AnsiConsole.MarkupLine(new ColorDiagnosticMessageFormatter().Format(message));
visitor.Settings = new ScratchScriptVisitorSettings('\n');
visitor.Visit(parser.program());
Console.WriteLine(visitor.Output);
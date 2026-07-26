using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Antlr4.Runtime;
using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.ProjectEmitter;
using ScratchScript.Compiler.ProjectEmitter.Helpers;
using ScratchScript.Compiler.ProjectEmitter.Models;
using ScratchScript.Compiler.Rewriters.Codegen.HighLevel;
using ScratchScript.Compiler.Rewriters.Codegen.LowLevel;
using ScratchScript.Compiler.Rewriters.TargetLowering;
using ScratchScript.Compiler.TypeChecker;
using Spectre.Console;
using JsonSerializer = System.Text.Json.JsonSerializer;
using ScratchScriptVisitor = ScratchScript.Compiler.AST.Builder.ScratchScriptVisitor;

const string source = """
                      namespace "meow";
                      
                      @inline function sayTimed(message: string, duration: number) {
                        __raw("looks_sayforsecs", {
                          inputs: {
                              MESSAGE: message,
                              SECS: duration
                          }
                        });
                      }

                      @inline function size() {
                        return __raw_expr("looks_size", {}, "number");
                      }

                      on start {
                      }
                      """;
var id = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(source))).ToString("N");
var symbols = new SymbolsStorage();

Console.WriteLine("constructing AST");

var inputStream = new AntlrInputStream(source);
var lexer = new ScratchScriptLexer(inputStream);
var tokenStream = new CommonTokenStream(lexer);
var parser = new ScratchScriptParser(tokenStream);
var visitor = new ScratchScriptVisitor(symbols);
DiagnosticReporter.Instance.Reported +=
    message => AnsiConsole.MarkupLine(new ColorDiagnosticMessageFormatter().Format(message));

var result = (IrProgramNode)visitor.Visit(parser.program());
if (!visitor.Success) return 1;

void RunUntilNoChanges(Type rewriter)
{
    Console.Write($"-> {rewriter.Name}");
    if (!rewriter.IsSubclassOf(typeof(IrRewriter))) throw new Exception();
    var hash = IrHasher.GetNodeHash(result);
    var count = 0;
    while (true)
    {
        count++;
        var nextResult = (IrProgramNode)((IrRewriter)Activator.CreateInstance(rewriter)!).VisitProgram(result);
        var nextHash = IrHasher.GetNodeHash(nextResult);
        Console.WriteLine($"{hash}, {nextHash}");
        if (nextHash == hash) break;
        hash = nextHash;
        result = nextResult;
    }

    Console.WriteLine($" ({count})");
}

Console.WriteLine("running type checker");
var typeChecker = new ScratchScriptTypeChecker();
result = (IrProgramNode)typeChecker.VisitProgram(result);
if (!typeChecker.Success) return 1;

Console.WriteLine("running high-level optimizations");
RunUntilNoChanges(typeof(RawFunctionsExpansionRewriter));
RunUntilNoChanges(typeof(ControlFlowDesugarizationRewriter));
RunUntilNoChanges(typeof(FunctionInlineRewriter));

Console.WriteLine("running lowering pass");
RunUntilNoChanges(typeof(Scratch3LoweringPass));

Console.WriteLine("running low-level optimizations");
RunUntilNoChanges(typeof(ComplexExpressionUnwindingRewriter));
RunUntilNoChanges(typeof(LoopSynthesisRewriter));
result = (IrProgramNode)new OperatorUnwindingRewriter().VisitProgram(result);
result = (IrProgramNode)new UnusedFunctionsRemovalRewriter().VisitProgram(result);

/*var bin = IrTreeSerializer.Serialize(result);
File.WriteAllBytes("test.bin", bin);*/

Console.WriteLine("packing into an archive");
var emitter = new ScratchScriptProjectEmitter(id);
emitter.VisitProgram(result);

var target = emitter.Target;
target.LayerOrder = 1;
target.Name = lexer.SourceName;
target.Costumes.Add(CostumeHelper.GetEmptyCostume());

var project = new Project();
project.Targets.Add(new Stage
{
    Name = "Stage",
    IsStage = true,
    LayerOrder = 0,
    Costumes = [CostumeHelper.GetEmptyCostume()]
});
project.Targets.Add(target);

var json = JsonSerializer.Serialize(project, new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    IncludeFields = true
});

if (File.Exists("output.sb3")) File.Delete("output.sb3");
using var archive = ZipFile.Open("output.sb3", ZipArchiveMode.Create);
var projectEntry = archive.CreateEntry("project.json");
using var projectWriter = new StreamWriter(projectEntry.Open());
projectWriter.Write(json);
projectWriter.Close();

foreach (var costume in project.Targets.SelectMany(t => t.Costumes)
             .DistinctBy(c => c.Md5Extension))
{
    var entry = archive.CreateEntry(costume.Md5Extension);
    using var entryWriter = new StreamWriter(entry.Open());
    entryWriter.BaseStream.Write(costume.Data, 0, costume.Data.Length);
    entryWriter.Close();
}

Console.WriteLine("done");
return 0;
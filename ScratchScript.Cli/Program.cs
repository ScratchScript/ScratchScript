using System.Security.Cryptography;
using System.Text;
using Antlr4.Runtime;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ScratchScript.Compiler.AST.GeneratedVisitor;
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
using ScratchScriptVisitor = ScratchScript.Compiler.AST.Builder.ScratchScriptVisitor;

const string source = """
                      function fibonacci(x: number): number {
                        return x < 2 ? x: fibonacci(x - 1) + fibonacci(x - 2);
                      }

                      on start { 
                        for(let i = fibonacci(8); i < 100; i += 1) {
                            let count = 5;
                            while(count > 0) {
                                count -= 1;
                                __raw("looks_sayforsecs", {
                                    inputs: {
                                        MESSAGE: `fibonacci(${i}) = ${fibonacci(i)}\ncount = ${count / 10}`,
                                        SECS: count / 10
                                    }
                                });
                                if(count <= 1) break;
                            }
                        }
                      }
                      """;
var id = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(source))).ToString("N");

Console.WriteLine("constructing AST");

var inputStream = new AntlrInputStream(source);
var lexer = new ScratchScriptLexer(inputStream);
var tokenStream = new CommonTokenStream(lexer);
var parser = new ScratchScriptParser(tokenStream);
var visitor = new ScratchScriptVisitor();
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

Console.WriteLine("running lowering pass");
RunUntilNoChanges(typeof(Scratch3LoweringPass));

Console.WriteLine("running low-level optimizations");
RunUntilNoChanges(typeof(ComplexExpressionUnwindingRewriter));
RunUntilNoChanges(typeof(LoopSynthesisRewriter));
result = (IrProgramNode)new OperatorUnwindingRewriter().VisitProgram(result);

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

var contractResolver = new DefaultContractResolver
{
    NamingStrategy = new CamelCaseNamingStrategy()
};
var json = JsonConvert.SerializeObject(project, new JsonSerializerSettings
{
    ContractResolver = contractResolver,
    Formatting = Formatting.Indented
});

var archive = new ZipFile();
archive.AddEntry("project.json", json);
foreach (var costume in project.Targets.SelectMany(t => t.Costumes)
             .DistinctBy(c => c.Md5Extension))
    archive.AddEntry(costume.Md5Extension, costume.Data);
archive.Save("output.sb3");
archive.Dispose();

Console.WriteLine("done");
return 0;
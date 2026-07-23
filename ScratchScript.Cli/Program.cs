using System.Security.Cryptography;
using System.Text;
using Antlr4.Runtime;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Helpers;
using ScratchScript.Compiler.ProjectEmitter;
using ScratchScript.Compiler.ProjectEmitter.Models;
using ScratchScript.Compiler.Rewriters.Optimizations.HighLevel;
using ScratchScript.Compiler.Rewriters.Optimizations.LowLevel;
using ScratchScript.Compiler.Rewriters.TargetLowering;
using ScratchScript.Compiler.TypeChecker;
using Spectre.Console;
using ScratchScriptVisitor = ScratchScript.Compiler.AST.Builder.ScratchScriptVisitor;

const string source = """
                      function id(x: number) { return x; }
                      
                      on start { 
                        let count = 0;
                        while(id(count) < 100) {
                            count += 1;
                            if(count % 2 == 0) continue;
                            if(count == 97) break;
                            let secs = 0.5;
                            while(secs >= 0.1) {
                                if(secs <= 0.21) break;
                                __raw("looks_sayforsecs", {inputs: {MESSAGE: count + secs, SECS: secs}});
                                secs -= 0.1;
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
RunUntilNoChanges(typeof(SyntheticLoopUnwindingRewriter));
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
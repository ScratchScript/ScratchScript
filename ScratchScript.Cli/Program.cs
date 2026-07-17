using System.Security.Cryptography;
using System.Text;
using Antlr4.Runtime;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ScratchScript.Compiler.Backend.Emitter;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Backend.Rewriters.Optimizations.HighLevel;
using ScratchScript.Compiler.Backend.Rewriters.Optimizations.LowLevel;
using ScratchScript.Compiler.Backend.Rewriters.TargetLowering;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Helpers;
using ScratchScript.Compiler.Models;
using Spectre.Console;

const string source = """
                      function factorial(n: number): number {
                        return n <= 1 ? 1: n * factorial(n - 1);
                      }

                      on start {
                        //let a = 3;
                        __raw("looks_sayforsecs", {inputs: {MESSAGE: factorial(5) + factorial(2), SECS: factorial(3)}});
                        //let b = __raw_expr("operator_add", {inputs: {NUM1: 2, NUM2: a}}, "number");
                      }
                      """;
var id = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(source))).ToString("N");

var inputStream = new AntlrInputStream(source);
var lexer = new ScratchScriptLexer(inputStream);
var tokenStream = new CommonTokenStream(lexer);
var parser = new ScratchScriptParser(tokenStream);
var visitor = new ScratchScriptVisitor();
visitor.DiagnosticReporter.Reported +=
    message => AnsiConsole.MarkupLine(new ColorDiagnosticMessageFormatter().Format(message));
var result = (IrProgramNode)visitor.Visit(parser.program());

void RunUntilNoChanges(Type rewriter)
{
    if (!rewriter.IsSubclassOf(typeof(IrRewriter))) throw new Exception();
    var hash = IrHasher.GetNodeHash(result);
    while (true)
    {
        var nextResult = (IrProgramNode)((IrRewriter)Activator.CreateInstance(rewriter)!).VisitProgram(result);
        var nextHash = IrHasher.GetNodeHash(nextResult);
        if (nextHash == hash) break;
        hash = nextHash;
        result = nextResult;
    }
}


Console.WriteLine(IrHasher.GetNodeHash(result));
RunUntilNoChanges(typeof(RawFunctionsExpansionRewriter));
Console.WriteLine(IrHasher.GetNodeHash(result));
//Console.WriteLine(ObjectDumper.Dump(result, DumpStyle.CSharp));
Console.WriteLine("running lowering pass");
RunUntilNoChanges(typeof(Scratch3LoweringPass));
//Console.WriteLine(ObjectDumper.Dump(result, DumpStyle.CSharp));
Console.WriteLine("running operator unwinder");
Console.WriteLine(IrHasher.GetNodeHash(result));
RunUntilNoChanges(typeof(ComplexExpressionUnwindingRewriter));

result = (IrProgramNode)new OperatorUnwindingRewriter().VisitProgram(result);
Console.WriteLine(IrHasher.GetNodeHash(result));
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
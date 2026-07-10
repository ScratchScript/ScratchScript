using System.Security.Cryptography;
using System.Text;
using Antlr4.Runtime;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ScratchScript.Compiler.Backend.Emitter;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Backend.Rewriters.TargetLowering;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Helpers;
using ScratchScript.Compiler.Models;
using Spectre.Console;

const string source = """
                      on start {
                        let c = "test";
                        let a = 3;
                        __raw("control_wait", {inputs: {DURATION: a}});
                        let b = __raw_expr("operator_add", {inputs: {NUM1: 2, NUM2: a}}, "number");
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

Console.WriteLine(ObjectDumper.Dump(result, DumpStyle.CSharp));
Console.WriteLine("running lowering pass");
result = (IrProgramNode)new Scratch3LoweringPass().VisitProgram(result);
Console.WriteLine(ObjectDumper.Dump(result, DumpStyle.CSharp));
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
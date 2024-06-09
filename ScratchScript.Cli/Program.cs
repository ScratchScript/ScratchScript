using Antlr4.Runtime;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ScratchScript.Compiler.Backend.GeneratedVisitor;
using ScratchScript.Compiler.Backend.Implementation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Implementation;
using ScratchScript.Compiler.Helpers;
using ScratchScript.Compiler.Models;
using Spectre.Console;

const string source = """
                      on start {
                        let a = 22;
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

var output = visitor.Output;
Console.WriteLine(output);

var irInputStream = new AntlrInputStream(output);
var irLexer = new ScratchIRLexer(irInputStream);
var irTokenStream = new CommonTokenStream(irLexer);
var irParser = new ScratchIRParser(irTokenStream);
var irVisitor =
    new ScratchIRVisitor(new ScratchIRVisitorSettings(visitor.Id, visitor.Target is CompilerTarget.Scratch3));
irVisitor.Visit(irParser.program());

Console.WriteLine("packing into an archive");

var target = irVisitor.Target;
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
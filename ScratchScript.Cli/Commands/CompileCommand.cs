using System.ComponentModel;
using System.IO.Compression;
using System.Text.Json;
using ScratchScript.Cli.Models;
using ScratchScript.Cli.Utils;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.ProjectEmitter.Helpers;
using ScratchScript.Compiler.ProjectEmitter.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ScratchScript.Cli.Commands;

public class CompileCommand : Command<CompileCommand.Settings>
{
    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(settings.ProjectPath))
        {
            AnsiConsole.MarkupLine(
                $"[red]error:[/] project directory ({Path.GetFullPath(settings.ProjectPath)}) doesn't exist");
            return 1;
        }

        Directory.SetCurrentDirectory(settings.ProjectPath);

        var config = TryGetProjectConfiguration();
        if (config is null) return 1;
        var sourceFiles = TryGetSourceFiles();
        if (sourceFiles is null) return 1;

        AnsiConsole.WriteLine("configuring output directory");
        CopyDirectoryStructure("src", "out");

        DiagnosticReporter.Instance.Reported +=
            message => AnsiConsole.MarkupLine('\n' + new ColorDiagnosticMessageFormatter().Format(message));

        var symbols = new SymbolsStorage();
        var targets = new List<Target>();
        var emitTarget = config.Type == ProjectType.Application;

        AnsiConsole.WriteLine("populating symbols storage");
        var nodes = new List<(string, IrProgramNode)>();
        foreach (var file in sourceFiles)
        {
            var (success, node) = PopulateSymbols(symbols, file);
            if (!success || node is null) return 1;
            nodes.Add((file, node));
        }

        AnsiConsole.WriteLine("compiling source code");
        foreach (var (file, node) in nodes)
        {
            var (success, target) = CompileFromTree(node, file, symbols, emitTarget);
            if (!success) return 1;
            if (emitTarget)
            {
                if (target is null) return 1;
                targets.Add(target);
            }
        }

        try
        {
            var serializedSymbols = SymbolsStorageSerializer.Serialize(symbols);
            var symbolsPath = Path.Join("out", $"{config.Name}.symbols");
            File.WriteAllBytes(symbolsPath, serializedSymbols);
            AnsiConsole.WriteLine($"saved symbols to {symbolsPath}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(
                "[red]failed to write the symbols file[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
        }

        if (emitTarget)
            try
            {
                var (bundlePath, time) = Benchmarker.Measure(() => BundleProjectFile(config.Name, targets));
                AnsiConsole.WriteLine($"saved project bundle to {bundlePath} ({time}ms)");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(
                    "[red]failed to create the project bundle[/]");
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            }

        AnsiConsole.WriteLine("done!");
        return 0;
    }

    private static string BundleProjectFile(string name, IEnumerable<Target> targets)
    {
        var project = new Project();
        project.Targets.Add(new Stage
        {
            Name = "Stage",
            IsStage = true,
            LayerOrder = 0,
            Costumes = [CostumeHelper.GetEmptyCostume()]
        });
        foreach (var target in targets)
            project.Targets.Add(target);

        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true
        });

        var bundlePath = Path.Join("out", $"{name}.sb3");
        if (File.Exists(bundlePath)) File.Delete(bundlePath);
        using var archive = ZipFile.Open(bundlePath, ZipArchiveMode.Create);
        var projectEntry = archive.CreateEntry("project.json");
        using var projectWriter = new StreamWriter(projectEntry.Open());
        projectWriter.Write(json);
        projectWriter.Close();

        foreach (var costume in project.Targets.SelectMany(t => t.Costumes)
                     .DistinctBy(c => c.Md5Extension))
        {
            var entry = archive.CreateEntry(costume.Md5Extension);
            using var stream = entry.Open();
            stream.Write(costume.Data, 0, costume.Data.Length);
            stream.Close();
        }

        return bundlePath;
    }

    private static (bool success, IrProgramNode? programNode) PopulateSymbols(SymbolsStorage symbols, string file)
    {
        Status($"parsing {file}");

        try
        {
            var ((visitor, initialProgramNode), astBuildTime) =
                Benchmarker.Measure(() => ScratchScriptCompilerUtils.BuildAst(symbols, file));
            if (!visitor.Success || initialProgramNode is null)
            {
                Status("[red]exiting due to malformed source code[/]", 2);
                return (false, null);
            }

            Status($"built AST ({astBuildTime}ms)", 2);
            return (true, initialProgramNode);
        }
        catch (Exception ex)
        {
            Status("[red]failed to run the AST builder[/]", 2);
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return (false, null);
        }
    }

    private static void Status(string markup, int offset = 1)
        => AnsiConsole.MarkupLine($"{new string('\t', offset)}-> {markup}");

    private static (bool success, Target? target) CompileFromTree(IrProgramNode programNode,
        string file, SymbolsStorage symbols, bool emitTarget)
    {
        Status($"compiling {file}");

        try
        {
            var ((programNodeWithImports, importerSuccessful), importerTime) =
                Benchmarker.Measure(() => ScratchScriptCompilerUtils.HandleImports(programNode, symbols));
            if (!importerSuccessful)
            {
                Status("[red]exiting due to import errors[/]", 2);
                return (false, null);
            }

            programNode = programNodeWithImports;
            Status($"imports handled ({importerTime}ms)", 2);
        }
        catch (Exception ex)
        {
            Status("[red]failed to run the imports handler[/]", 2);
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return (false, null);
        }

        try
        {
            var ((typeCheckedProgramNode, typeCheckerSuccessful), typeCheckerTime) =
                Benchmarker.Measure(() => ScratchScriptCompilerUtils.TypeCheck(programNode));
            if (!typeCheckerSuccessful)
            {
                Status("[red]exiting due to type errors[/]", 2);
                return (false, null);
            }

            programNode = typeCheckedProgramNode;
            Status($"typechecked ({typeCheckerTime}ms)", 2);
        }
        catch (Exception ex)
        {
            Status("[red]failed to run the type checker[/]", 2);
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return (false, null);
        }

        try
        {
            Status("running high-level codegen", 2);
            programNode = ScratchScriptCompilerUtils.RunCodegen(CodegenLevel.High,
                programNode,
                s => Status(s, 3));

            Status("running lowering pass", 2);
            programNode = ScratchScriptCompilerUtils.RunCodegen(CodegenLevel.LoweringPass,
                programNode,
                s => Status(s, 3));

            Status("running low-level codegen", 2);
            programNode = ScratchScriptCompilerUtils.RunCodegen(CodegenLevel.Low,
                programNode,
                s => Status(s, 3));
        }
        catch (Exception ex)
        {
            Status("[red]failed to run one or more of the codegen passes[/]", 2);
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return (false, null);
        }

        try
        {
            var serialized = IrTreeSerializer.Serialize(programNode);
            var relativePath = Path.GetRelativePath("src", file);
            var targetRelativePath = Path.ChangeExtension(relativePath, ".cscrs");
            var path = Path.Combine("out", targetRelativePath);
            File.WriteAllBytes(path, serialized);
            Status($"saved to {path}", 2);
        }
        catch (Exception ex)
        {
            Status("[red]failed to save the bytecode to a file[/]", 2);
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return (false, null);
        }

        if (!emitTarget) return (true, null);

        try
        {
            var (target, emitTargetTime) =
                Benchmarker.Measure(() => ScratchScriptCompilerUtils.EmitTarget(programNode, file));
            Status($"target emitted ({emitTargetTime}ms)", 2);
            return (true, target);
        }
        catch (Exception ex)
        {
            Status("[red]failed to emit a scratch target[/]", 2);
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
            return (false, null);
        }
    }

    private static void CopyDirectoryStructure(string sourceDirectory, string destinationDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new InvalidOperationException("Cannot copy the structure of a non-existent directory");
        if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);

        var subdirectories = Directory.EnumerateDirectories(sourceDirectory);
        foreach (var subdirectory in subdirectories)
        {
            var name = new DirectoryInfo(subdirectory).Name;
            CopyDirectoryStructure(Path.Combine(sourceDirectory, name), Path.Combine(destinationDirectory, name));
        }
    }


    private static List<string>? TryGetSourceFiles()
    {
        if (!Directory.Exists("src"))
        {
            AnsiConsole.MarkupLine(
                "[red]error:[/] project doesn't have a src folder, nothing to compile");
            return null;
        }

        var sourceFiles = Directory.EnumerateFiles("src", "*.scrs", SearchOption.AllDirectories).ToList();
        if (sourceFiles.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[red]error:[/] no source files found to compile");
            return null;
        }

        return sourceFiles;
    }

    private static ProjectConfiguration? TryGetProjectConfiguration()
    {
        if (!File.Exists("project.json"))
        {
            AnsiConsole.MarkupLine(
                "[red]error:[/] project directory doesn't contain a project.json file");
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ProjectConfiguration>(File.ReadAllText("project.json"),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        catch
        {
            AnsiConsole.MarkupLine(
                "[red]error:[/] failed to parse the project's configuration file (project.json)");
            return null;
        }
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[path]")]
        [Description("The path to the project to compile")]
        [DefaultValue(".")]
        public string ProjectPath { get; init; } = ".";
    }
}
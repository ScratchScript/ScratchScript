using ScratchScript.Compiler;
using Serilog;
using Serilog.Core;
using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(x =>
{
	x.PropagateExceptions();
	x.AddCommand<BuildCommand>("build")
		.WithAlias("compile")
		.WithDescription("Converts a .scrs file into a .sb3 project.")
		.WithExample(new[] {"build", "test.scrs"})
		.WithExample(new[] {"build", "test.scrs", "--output", "project.sb3"});
});

app.Run(args);

class BuildCommand : Command<BuildCommand.BuildCommandSettings>
{
	public class BuildCommandSettings : CommandSettings
	{
		[CommandArgument(0, "[path]")]
		public string? Path { get; set; }
	
		[CommandOption("-o|--output")]
		public string? Output { get; set; }
	}

	public override int Execute(CommandContext context, BuildCommandSettings settings)
	{
		if (string.IsNullOrEmpty(settings.Path))
		{
			AnsiConsole.MarkupLine("[bold red]Please specify the source file![/]");
			return 1;
		}
		
		if (!File.Exists(settings.Path))
		{
			AnsiConsole.MarkupLine($"[bold red]Input file \"{settings.Path}\" does not exist![/]");
			return 1;
		}

		if (string.IsNullOrEmpty(settings.Output))
			settings.Output = Path.GetFileNameWithoutExtension(settings.Path) + ".sb3";
		if (!string.IsNullOrEmpty(settings.Output) && File.Exists(settings.Output))
		{
			if (AnsiConsole.Confirm("[yellow]Output file already exists. Delete it?[/]"))
				File.Delete(settings.Output);
			else return 0;
		}
		
		Log.Logger = new LoggerConfiguration()
			.WriteTo.File($"log_{DateTime.Now:ddMMyyyyHHmm}.txt")
			.WriteTo.Console()
			#if DEBUG
			.MinimumLevel.Debug()
			#else
			.MinimumLevel.Information()
			#endif
			.CreateLogger();
		Log.Information("Logger initialized");
		Log.Information("Preparing to build \"{SourceFile}\"..", settings.Path);
		
		var compiler = new ProjectCompiler(settings.Path, settings.Output);
		
		compiler.Initialize();
		compiler.Compile();
		compiler.Finish();
		
		AnsiConsole.MarkupLine("[green]Done![/]");
		return 0;
	}
}

//using ScratchScript.Core;
//using ScratchScript.Core.Model;

//https://www.cs.uic.edu/~i109/Notes/COperatorPrecedenceTable.pdf
/*var parser = new ScratchScriptParser();
var buildResult = new ParserBuilder<ScratchScriptToken, AST>().BuildParser(parser, ParserType.EBNF_LL_RECURSIVE_DESCENT, "statement");
//var parseResult = buildResult.Result.Parse("1 + ");
//Console.WriteLine(parseResult.SyntaxTree);
while (true)
{
	try
	{
		var code = Console.ReadLine();
		var parseResult = buildResult.Result.Parse(code);
		if (parseResult.IsOk)
		{
			Console.WriteLine("Ok");
			Console.WriteLine(parseResult.Result);
		}
		else Console.WriteLine($"Error\n{string.Join('\n', parseResult.Errors.Select(x => x.ErrorMessage))}");
	}
	catch(Exception e) {Console.WriteLine($"Failed\n{e.Message}\n{e.StackTrace}");}
}*/
/*while (true)
{
	var line = Console.ReadLine();

	
	
}*/
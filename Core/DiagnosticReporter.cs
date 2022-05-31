using Antlr4.Runtime;
using ScratchScript.Compiler;
using Serilog;
using Spectre.Console;

namespace ScratchScript.Core;

public static class DiagnosticReporter
{
	public static void ReportError(IToken position, string id, string additionalText = "", params object[] objects)
	{
		Log.Error("An error has been reported (Line {Line}, Position {Position}). Error ID: {ErrorId}", position.Line,
			position.Column, id);
		DefaultHandler("Error", "red", position, id, additionalText, objects);
		ProjectCompiler.Current.Success = false;
	}

	public static void ReportWarning(IToken position, string id, string additionalText = "", params object[] objects)
	{
		Log.Warning("A warning has been reported (Line {Line}, Position {Position}). Warning ID: {ErrorId}",
			position.Line, position.Column, id);
		DefaultHandler("Warning", "yellow", position, id, additionalText, objects);
	}

	private static void DefaultHandler(string name, string color, IToken position, string id,
		string additionalText = "", params object[] objects)
	{
		var project = ProjectCompiler.Current;
		var fileData = File.ReadAllLines(project.SourcePath);
		AnsiConsole.MarkupLine($"[{color}]{name}[/]: {string.Format(Strings.Messages[id], objects)}");
		AnsiConsole.WriteLine($" --> {project.FileName}:{position.Line}:{position.Column + 1}");
		var line = $"{position.Line} | ";
		AnsiConsole.WriteLine($"{line}{fileData[position.Line - 1]}");
		var underline = new string(' ', position.Column + line.Length) + new string('~', position.Text.Length);
		AnsiConsole.MarkupLine($"[{color}]{underline}[/]");
		if (!string.IsNullOrEmpty(additionalText))
			AnsiConsole.WriteLine("note: " + additionalText);
		AnsiConsole.MarkupLine($"\nFor more information, try `[yellow bold]ScratchScript explain {id}[/]`.");
	}
}
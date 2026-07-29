using System.Reflection;
using ScratchScript.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(c =>
{
    c.PropagateExceptions();
    c.SetApplicationName("scrs");
    c.SetApplicationVersion(Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "dev");
    c.AddCommand<CompileCommand>("compile");
});
return app.Run(args);
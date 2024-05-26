using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.Information;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor
{
    private Dictionary<string, ScratchCustomBlock> _functions = [];
    
    public override object? VisitFunctionBlock(ScratchIRParser.FunctionBlockContext context)
    {
        var name = context.Identifier().GetText();
        if (_functions.ContainsKey(name))
            throw new Exception($"A function with the name \"{name}\" has already been declared.");

        var function = new ScratchCustomBlock(name, context.WarpIdentifier() != null, GenerateBlockId);
        _functions[name] = function;

        var stack = VisitCommands(context.command());
        AttachStackToBlock(function.Definition, stack);

        Target.Blocks[function.Definition.Id] = function.Definition;
        Target.Blocks[function.Prototype.Id] = function.Prototype;
        
        return function.Definition;
    }

    public override object? VisitCallCommand(ScratchIRParser.CallCommandContext context)
    {
        var name = context.Identifier().GetText();
        if (!_functions.TryGetValue(name, out var function))
            throw new Exception($"No function with name \"{name}\" was found.");

        var call = function.Call.Clone();
        call.Id = GenerateBlockId(Function.Call);
        return call;
    }
}
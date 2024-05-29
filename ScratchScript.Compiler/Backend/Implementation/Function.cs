using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.Information;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Targets.Scratch3;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor
{
    private readonly Dictionary<string, ScratchCustomBlock> _functions = [];
    private string _currentFunction = "";

    public override object? VisitFunctionBlock(ScratchIRParser.FunctionBlockContext context)
    {
        var name = context.Identifier().GetText();
        if (_functions.ContainsKey(name))
            throw new Exception($"A function with the name \"{name}\" has already been declared.");

        var function = new ScratchCustomBlock(name, context.WarpIdentifier() != null, GenerateBlockId);
        if (Settings.UseStack)
        {
            function.AddReporter(ScratchCustomBlock.StackPointerName);
            function.AddReporter(ScratchCustomBlock.IntermediateStackPointerName);
        }

        _functions[name] = function;
        _currentFunction = name;

        var stack = VisitCommands(context.command());
        AttachStackToBlock(function.Definition, stack);

        UpdateFunction(function);
        _currentFunction = "";

        return function.Definition;
    }

    public override object? VisitCallCommand(ScratchIRParser.CallCommandContext context)
    {
        var name = context.Identifier().GetText();
        if (!_functions.TryGetValue(name, out var function))
            throw new Exception($"No function with name \"{name}\" was found.");

        var call = function.Call.Clone();
        call.Id = GenerateBlockId(Function.Call);

        if (Settings.UseStack)
        {
            var lengthOfStack = new Block
            {
                Opcode = Data.LengthOfList,
                Id = GenerateBlockId(Data.LengthOfList),
                Shadow = true,
                Fields =
                {
                    ["LIST"] = CreateField(Scratch3Helper.StackList)
                }
            };

            var lengthOfIntermediateStack = lengthOfStack.Clone();
            lengthOfIntermediateStack.Id = GenerateBlockId(Data.LengthOfList);
            lengthOfIntermediateStack.Fields["LIST"] = CreateField(Scratch3Helper.IntermediateStackList);

            Target.Blocks[lengthOfStack.Id] = lengthOfStack;
            Target.Blocks[lengthOfIntermediateStack.Id] = lengthOfIntermediateStack;

            call.Inputs[function.Reporters[ScratchCustomBlock.StackPointerName].Id] = CreateInput(lengthOfStack, call);
            call.Inputs[function.Reporters[ScratchCustomBlock.IntermediateStackPointerName].Id] =
                CreateInput(lengthOfIntermediateStack, call);
        }

        return call;
    }

    public override object? VisitArgumentExpression(ScratchIRParser.ArgumentExpressionContext context)
    {
        var name = context.argumentIdentifier().Identifier().GetText();

        if (string.IsNullOrEmpty(_currentFunction))
            throw new Exception("Cannot use the stack reporter expression in a non-function context.");
        if (!_functions[_currentFunction].Reporters.ContainsKey(name))
            throw new Exception($"The function did not have the argument \"{name}\" defined.");

        var reporter = _functions[_currentFunction].Reporters[name].Clone();
        reporter.Id = GenerateBlockId(Function.ReporterStringNumber);
        Target.Blocks[reporter.Id] = reporter;
        return reporter;
    }

    private void UpdateFunction(ScratchCustomBlock function)
    {
        Target.Blocks[function.Definition.Id] = function.Definition;
        Target.Blocks[function.Prototype.Id] = function.Prototype;
        foreach (var (_, block) in function.Reporters)
            Target.Blocks[block.Id] = block;
    }
}
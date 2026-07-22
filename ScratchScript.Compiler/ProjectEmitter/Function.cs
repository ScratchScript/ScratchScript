using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.ProjectEmitter.Blocks;
using ScratchScript.Compiler.ProjectEmitter.Models;

namespace ScratchScript.Compiler.ProjectEmitter;

public partial class ScratchScriptProjectEmitter
{
    private readonly Dictionary<Guid, ScratchCustomBlock> _functions = [];
    private Guid _currentFunction = Guid.Empty;

    // TODO: refactor this
    private readonly List<IrFunctionNode> _revisitFunctions = [];

    public override object? VisitFunction(IrFunctionNode node)
    {
        if (_functions.ContainsKey(node.FunctionScope.Id))
            throw new Exception($"A function with the ID \"{node.FunctionScope.Id}\" has already been declared.");

        var function = new ScratchCustomBlock(node.FunctionScope.FunctionName, node.Warp, GenerateBlockId);
        if (node.FunctionScope.UseArgumentReporters)
            foreach (var arg in node.FunctionScope.Arguments)
                function.AddStringNumberReporter(arg.Name);

        _functions[node.FunctionScope.Id] = function;
        _revisitFunctions.Add(node);
        return function.Definition;
    }

    private void RevisitFunction(IrFunctionNode node)
    {
        _currentFunction = node.FunctionScope.Id;
        var function = _functions[node.FunctionScope.Id];
        var stack = VisitScope(node.FunctionScope);
        AttachStackToBlock(function.Definition, stack);
        UpdateFunction(function);
        _currentFunction = Guid.Empty;
    }

    public override object? VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
    {
        // TODO: improve exceptions
        if (_currentFunction == Guid.Empty || !_functions[_currentFunction].Reporters.ContainsKey(node.Name))
            throw new Exception();

        var reporter = _functions[_currentFunction].Reporters[node.Name].Clone();
        reporter.Id = GenerateBlockId(Function.ReporterStringNumber);
        Target.Blocks[reporter.Id] = reporter;
        return reporter;
    }

    public override object? VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node) =>
        throw new NotImplementedException();

    public override object? VisitFunctionReturnCommandNode(IrReturnCommandNode node) =>
        throw new NotImplementedException();

    public override object? VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        var arguments = node.Arguments.ToList();
        var function = _functions.Values.First(f => f.Name == node.Function);
        var call = function.Call.Clone();
        call.Id = GenerateBlockId(Function.Call);

        for (var idx = 0; idx < arguments.Count; idx++)
        {
            var value = Visit(arguments[idx]);
            if (value == null) return null;
            call.Inputs[function.Reporters.GetAt(idx).Value.Id] =
                value is Block valueBlock ? CreateInput(valueBlock, call) : CreateInput(value);
        }

        return call;
    }

    private void UpdateFunction(ScratchCustomBlock function)
    {
        Target.Blocks[function.Definition.Id] = function.Definition;
        Target.Blocks[function.Prototype.Id] = function.Prototype;
        foreach (var (_, block) in function.Reporters)
            Target.Blocks[block.Id] = block;
    }
}
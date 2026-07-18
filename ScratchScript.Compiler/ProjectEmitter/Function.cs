using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.ProjectEmitter.Blocks;

namespace ScratchScript.Compiler.ProjectEmitter;

public partial class ScratchScriptProjectEmitter
{
    private readonly Dictionary<string, ScratchCustomBlock> _functions = [];
    private string _currentFunction = "";

    public override object? VisitFunction(IrFunctionNode node)
    {
        if (_functions.ContainsKey(node.FunctionScope.Id))
            throw new Exception($"A function with the ID \"{node.FunctionScope.Id}\" has already been declared.");

        var function = new ScratchCustomBlock(node.FunctionScope.FunctionName, node.Warp, GenerateBlockId);
        if (node.FunctionScope.UseArgumentReporters)
            foreach (var arg in node.FunctionScope.Arguments)
                function.AddStringNumberReporter(arg.Name);

        _functions[node.FunctionScope.Id] = function;
        _currentFunction = node.FunctionScope.Id;

        var stack = VisitScope(node.FunctionScope);
        AttachStackToBlock(function.Definition, stack);
        UpdateFunction(function);
        _currentFunction = "";

        return function.Definition;
    }

    public override object? VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
    {
        // TODO: improve exceptions
        if (string.IsNullOrEmpty(_currentFunction) || !_functions[_currentFunction].Reporters.ContainsKey(node.Name))
            throw new Exception();

        var reporter = _functions[_currentFunction].Reporters[node.Name].Clone();
        reporter.Id = GenerateBlockId(Function.ReporterStringNumber);
        Target.Blocks[reporter.Id] = reporter;
        return reporter;
    }

    public override object? VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public override object? VisitFunctionReturnCommandNode(IrFunctionReturnCommandNode node)
    {
        throw new NotImplementedException();
    }

    private void UpdateFunction(ScratchCustomBlock function)
    {
        Target.Blocks[function.Definition.Id] = function.Definition;
        Target.Blocks[function.Prototype.Id] = function.Prototype;
        foreach (var (_, block) in function.Reporters)
            Target.Blocks[block.Id] = block;
    }
}
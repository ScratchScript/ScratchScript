using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Rewriters.Informational;

namespace ScratchScript.Compiler.Rewriters.Codegen.HighLevel;

internal record InliningInfo(IEnumerable<IrExpressionNode> Arguments, bool UseTemporaryVariable, IrNode? ReturnValue);

public class FunctionInlineRewriter : IrRewriter
{
    // TODO: refactor to a stack of pushable state
    private InliningInfo? _info;

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        var visitedArguments =
            node.Arguments.Select(Visit).OfType<IrExpressionNode>().ToList();

        var function = ProgramNode.Functions.FirstOrDefault(f => f.FunctionScope.FunctionName == node.Function);
        if (function == null) throw new Exception();
        if (!(function.Attributes?.Any(a => a.Name == FunctionAttributes.AlwaysInlineFunction) ?? false))
            return node with { Arguments = visitedArguments };

        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope?.FunctionName == function.FunctionScope.FunctionName)
            throw new Exception("recursive call of inline function");

        var previousInfo = _info;
        _info = new InliningInfo(visitedArguments, false, null);
        if (VisitBlock(function) is not IrFunctionNode visitedFunction) throw new Exception();
        _info = previousInfo;
        return new IrCommandSequenceNode(visitedFunction.FunctionScope.Body);
    }

    public override IrNode VisitFunctionCallExpression(IrFunctionCallExpressionNode node)
    {
        var visitedArguments =
            node.Arguments.Select(Visit).OfType<IrExpressionNode>().ToList();

        var function = ProgramNode.Functions.FirstOrDefault(f => f.FunctionScope.FunctionName == node.Function);
        if (function == null || ReservedNames.GlobalCallableFunctions.Contains(node.Function) ||
            !(function.Attributes?.Any(a => a.Name == FunctionAttributes.AlwaysInlineFunction) ?? false))
            return node with { Arguments = visitedArguments };

        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope?.FunctionName == function.FunctionScope.FunctionName)
            throw new Exception("recursive call of inline function");

        var counter = new ReturnStatementCountRewriter();
        counter.Visit(function);

        var previousInfo = _info;
        _info = new InliningInfo(visitedArguments, counter.ScopesWithReturnStatements.Count > 1, null);
        if (CurrentScope == null) throw new Exception();
        if (_info.UseTemporaryVariable && CurrentScope.GetVariable(ReservedNames.TemporaryReturnValue) == null)
            CurrentScope.Variables.Add(new ScratchScriptVariable(ReservedNames.TemporaryReturnValue));
        if (VisitBlock(function) is not IrFunctionNode) throw new Exception();

        var result = _info.UseTemporaryVariable
            ? new IrLocalVariableIdentifierExpressionNode(ReservedNames.TemporaryReturnValue)
            : _info.ReturnValue ?? throw new Exception();
        _info = previousInfo;
        return result;
    }

    public override IrNode VisitFunctionReturnCommand(IrReturnCommandNode node)
    {
        if (_info is null) return base.VisitFunctionReturnCommand(node);
        if (base.VisitFunctionReturnCommand(node) is not IrReturnCommandNode visitedReturn) throw new Exception();
        if (visitedReturn.ReturnValue == null) return visitedReturn;
        if (!_info.UseTemporaryVariable) _info = _info with { ReturnValue = visitedReturn.ReturnValue };

        return _info.UseTemporaryVariable
            ? new IrSetCommandNode(ReservedNames.TemporaryReturnValue,
                visitedReturn.ReturnValue)
            : new IrNoOpCommandNode();
    }

    public override IrNode VisitFunctionArgumentExpression(IrFunctionArgumentExpressionNode node)
    {
        if (_info is null) return base.VisitFunctionArgumentExpression(node);
        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope is null)
            throw new Exception();
        var index = closestFunctionScope.Arguments.FindIndex(v => v.Name == node.Name);
        return _info.Arguments.ElementAt(index);
    }
}
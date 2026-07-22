using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;

namespace ScratchScript.Compiler.Rewriters.Informational;

// intended to be called per-scope so there's one public field and not a dictionary
// TODO: most likely not the best implementation
public class ScopeTotalVariableCountCalculationRewriter : IrRewriter
{
    public int TotalVariableCount { get; private set; }

    private Scope? _target;
    private readonly Dictionary<Guid, int> _contenders = [];
    private readonly Dictionary<Guid, List<Scope>> _children = [];

    public override IrNode VisitBlock(IrBlockNode node)
    {
        _target ??= node.Scope;
        var isTarget = _target == node.Scope;

        var previousScope = CurrentScope;
        CurrentScope = node.Scope;

        var result = (IrBlockNode)base.VisitBlock(node);
        CurrentScope = previousScope;

        var parentKey = result.Scope.ParentScope?.Id ?? Guid.Empty;
        if (!_children.ContainsKey(parentKey)) _children.Add(parentKey, []);
        _children[parentKey].Add(result.Scope);
        _contenders[parentKey] = Math.Max(result.Scope.Variables.Count, _contenders.GetValueOrDefault(parentKey, 0));

        if (isTarget) TotalVariableCount = CalculateContribution(_target);
        return result;
    }

    private int CalculateContribution(Scope scope)
    {
        var local = scope.Variables.Count;
        if (!_children.TryGetValue(scope.Id, out var childrenScopes) || childrenScopes.Count == 0) return local;
        return local + childrenScopes.Max(CalculateContribution);
    }
}
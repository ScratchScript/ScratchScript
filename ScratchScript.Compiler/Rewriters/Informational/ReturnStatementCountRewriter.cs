using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;

namespace ScratchScript.Compiler.Rewriters.Informational;

// intended to be called per-scope so there's one public field and not a dictionary
public class ReturnStatementCountRewriter : IrRewriter
{
    public readonly HashSet<Guid> ScopesWithReturnStatements = [];
    private FunctionScope _targetScope = null!;

    public override IrNode VisitFunction(IrFunctionNode node)
    {
        ScopesWithReturnStatements.Clear();
        _targetScope = node.FunctionScope;
        return base.VisitFunction(node);
    }

    public override IrNode VisitFunctionReturnCommand(IrReturnCommandNode node)
    {
        if (CurrentScope != null && CurrentScope.GetClosestFunctionScope()?.Id == _targetScope.Id)
            ScopesWithReturnStatements.Add(CurrentScope.Id);
        return base.VisitFunctionReturnCommand(node);
    }
}
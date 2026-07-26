using ScratchScript.Compiler.AST.Representation;

namespace ScratchScript.Compiler.Rewriters.Codegen.LowLevel;

public class UnusedFunctionsRemovalRewriter : IrRewriter
{
    private readonly HashSet<string> _calls = [];

    public override IrNode VisitProgram(IrProgramNode node)
    {
        var result = (IrProgramNode)base.VisitProgram(node);
        return result with { Functions = result.Functions.Where(f => _calls.Contains(f.FunctionScope.FunctionName)).ToList() };
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        _calls.Add(node.Function);
        return base.VisitCallFunctionCommand(node);
    }

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        _calls.Add(node.Function);
        return base.VisitFunctionCallExpressionNode(node);
    }
}
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.Rewriters.Codegen.LowLevel;

public class UnusedFunctionsRemovalRewriter : IrRewriter
{
    private const string SkipRewriterAttribute = "unusedFunctions";
    private readonly HashSet<string> _calls = [];

    public override IrNode VisitProgram(IrProgramNode node)
    {
        var result = (IrProgramNode)base.VisitProgram(node);
        return result.HasAttributeWithArgument(ProgramAttributes.SkipCompilerFeature, SkipRewriterAttribute)
            ? result
            : result with
            {
                TopLevelNodes = result.TopLevelNodes
                    .Where(f => f is not IrFunctionNode ff || _calls.Contains(ff.FunctionScope.FunctionName)).ToList()
            };
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        _calls.Add(node.Function);
        return base.VisitCallFunctionCommand(node);
    }

    public override IrNode VisitFunctionCallExpression(IrFunctionCallExpressionNode node)
    {
        _calls.Add(node.Function);
        return base.VisitFunctionCallExpression(node);
    }
}
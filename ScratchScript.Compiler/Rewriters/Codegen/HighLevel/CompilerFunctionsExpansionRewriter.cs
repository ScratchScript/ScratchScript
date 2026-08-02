using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.Rewriters.Codegen.HighLevel;

public class CompilerFunctionsExpansionRewriter : IrRewriter
{
    public const string SkipAttribute = "compilerFunctionsExpansion";

    public override IrNode VisitFunctionCallExpression(IrFunctionCallExpressionNode node)
    {
        var visited = (IrFunctionCallExpressionNode)base.VisitFunctionCallExpression(node);
        if (ShouldSkip()) return visited;

        return node.Function switch
        {
            ReservedNames.IsConstFunction => new IrConstantExpressionNode(
                TypedValue.Boolean(Visit(node.Arguments[0]) is IrConstantExpressionNode)),
            _ => visited
        };
    }

    private bool ShouldSkip()
    {
        if (ProgramNode.HasAttributeWithArgument(ProgramAttributes.SkipCompilerFeature, SkipAttribute)) return true;
        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope is null) return false;

        var function = ProgramNode.Functions.FirstOrDefault(f => f.FunctionScope.Id == closestFunctionScope.Id);
        return function is not null &&
               function.HasAttributeWithArgument(FunctionAttributes.SkipCompilerFeature, SkipAttribute);
    }
}
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.AST.Representation.TargetSpecific;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Rewriters.Optimizations.HighLevel;
using ScratchScript.Compiler.Rewriters.TargetLowering;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.Rewriters.Optimizations.LowLevel;

public class SyntheticLoopUnwindingRewriter : IrRewriter, IScratch3ExtensionRewriter
{
    private readonly List<IrFunctionNode> _pendingFunctions = [];

    private int SyntheticWhileLoopCount =>
        ProgramNode.Functions.Count(f => f.FunctionScope.FunctionName.StartsWith(ReservedNames.WhileMainFunction));

    public override IrNode VisitProgram(IrProgramNode node)
    {
        var program = (IrProgramNode)base.VisitProgram(node);
        return _pendingFunctions.Count != 0
            ? program with { Functions = program.Functions.Concat(_pendingFunctions) }
            : program;
    }

    public override IrNode VisitTargetSpecificNode(ITargetSpecificNode node) =>
        ((IScratch3ExtensionRewriter)this).VisitScratch3SpecificNode(node, Visit);

    public IrNode VisitSynthesizedWhileLoop(IrScratch3SynthesizedWhileLoopNode node, Func<IrNode, IrNode> visitor)
    {
        if (CurrentScope == null) throw new Exception();
        if (node.Body.Scope is not LoopScope loopScope) throw new Exception();

        var newCondition = IrRewriterUtils.RewriteUntilNoChanges<IrExpressionNode>(
            new ComplexExpressionUnwindingRewriter(),
            node.Condition with
            {
                Expression = new IrBinaryExpressionNode(IrBinaryOperator.And, node.Condition.Expression,
                    new IrBinaryExpressionNode(IrBinaryOperator.Equal,
                        new IrGlobalVariableIdentifierExpressionNode(ReservedNames.ControlFlowBreak),
                        new IrConstantExpressionNode(TypedValue.Number(0))))
            }).ToComplex();

        var bodyFunction = new IrFunctionNode(true, new FunctionScope
        {
            FunctionName = $"{ReservedNames.WhileBodyFunction}_{SyntheticWhileLoopCount}",
            ReturnType = ScratchType.Void,
            Body = node.Body.Scope.Body,
            ParentScope = CurrentScope
        });

        var mainFunction = new IrFunctionNode(true, new FunctionScope
        {
            FunctionName = $"{ReservedNames.WhileMainFunction}_{SyntheticWhileLoopCount}",
            ReturnType = ScratchType.Void,
            ParentScope = CurrentScope
        });

        mainFunction.FunctionScope.Body =
        [
            newCondition.Dependencies ?? new IrNoOpCommandNode(),
            new IrSetCommandNode(ReservedNames.ControlFlowBreak, new IrConstantExpressionNode(TypedValue.Number(0))),
            new IrWhileCommandNode(newCondition.Expression, new IrBlockNode(new LoopScope
            {
                HasBreak = node.HasBreak,
                HasContinue = node.HasContinue,
                Body =
                [
                    new IrCallFunctionCommandNode(bodyFunction.FunctionScope.FunctionName, []),
                    loopScope.NextIterationPrerequisite ?? new IrNoOpCommandNode(),
                    newCondition.Cleanup ?? new IrNoOpCommandNode(),
                    newCondition.Dependencies ?? new IrNoOpCommandNode()
                ],
                ParentScope = mainFunction.FunctionScope
            })).WithFlag(LoopSynthesisRewriter.SyntheticWhileFlag),
            newCondition.Cleanup ?? new IrNoOpCommandNode(),
            new IrSetCommandNode(ReservedNames.ControlFlowBreak, new IrConstantExpressionNode(TypedValue.Number(0))),
        ];

        _pendingFunctions.Add(bodyFunction);
        _pendingFunctions.Add(mainFunction);
        return new IrCallFunctionCommandNode(mainFunction.FunctionScope.FunctionName, []);
    }

    public override IrNode VisitBreakCommand(IrBreakCommandNode node) => new IrCommandSequenceNode([
        new IrSetCommandNode(ReservedNames.ControlFlowBreak, new IrConstantExpressionNode(TypedValue.Number(1))),
        Scratch3CommandHelper.StopThisScript()
    ]);

    public override IrNode VisitContinueCommand(IrContinueCommandNode node) => Scratch3CommandHelper.StopThisScript();
}
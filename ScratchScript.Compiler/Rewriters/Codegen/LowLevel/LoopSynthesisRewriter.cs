using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.AST.Representation.TargetSpecific;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Rewriters.TargetLowering;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.Rewriters.Codegen.LowLevel;

public class LoopSynthesisRewriter : IrRewriter, IScratch3ExtensionRewriter
{
    public const string SyntheticWhileFlag = "SYNTHETIC_WHILE";
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

    public override IrNode VisitWhileCommand(IrWhileCommandNode node)
    {
        /*
         *  while loop structure:
         *
         *  {condition dependencies}
         *  while(condition) {
         *       {body}
         *       {condition cleanup}
         *       {condition dependencies} <- because it needs to be recalculated
         *  }
         *  {condition cleanup} <- if the loop exits, the leftover data is still there,
         *                         so it needs to be cleaned
         */
        if (node.Flags.Contains(SyntheticWhileFlag)) return node;

        if (CurrentScope == null) throw new Exception();
        if (Visit(node.Body) is not IrBlockNode { Scope: LoopScope loopScope } body) throw new Exception();
        if (Visit(node.Condition) is not IrExpressionNode condition) throw new Exception();

        // ideal case where we don't need to do anything
        if (condition is not IrComplexExpressionNode &&
            loopScope is { HasBreak: false, HasContinue: false })
        {
            if (loopScope.NextIterationPrerequisite != null) loopScope.Body.Add(loopScope.NextIterationPrerequisite);
            return new IrWhileCommandNode(condition, body).WithFlag(SyntheticWhileFlag);
        }

        var complexCondition = ((IrExpressionNode)Visit(node.Condition)).ToComplex();
        var newCondition = IrRewriterUtils.RewriteUntilNoChanges<IrExpressionNode>(
            new ComplexExpressionUnwindingRewriter(),
            complexCondition with
            {
                Expression = new IrBinaryExpressionNode(IrBinaryOperator.And, complexCondition.Expression,
                    new IrBinaryExpressionNode(IrBinaryOperator.Equal,
                        new IrGlobalVariableIdentifierExpressionNode(ReservedNames.ControlFlowBreak),
                        new IrConstantExpressionNode(TypedValue.Number(0))))
            }).ToComplex();

        var bodyFunction = new IrFunctionNode(true, new FunctionScope
        {
            FunctionName = $"{ReservedNames.WhileBodyFunction}_{SyntheticWhileLoopCount}",
            ReturnType = ScratchType.Void,
            Body = loopScope.Body,
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
                HasBreak = loopScope.HasBreak,
                HasContinue = loopScope.HasContinue,
                Body =
                [
                    new IrCallFunctionCommandNode(bodyFunction.FunctionScope.FunctionName, []),
                    loopScope.NextIterationPrerequisite ?? new IrNoOpCommandNode(),
                    newCondition.Cleanup ?? new IrNoOpCommandNode(),
                    newCondition.Dependencies ?? new IrNoOpCommandNode()
                ],
                ParentScope = mainFunction.FunctionScope
            })).WithFlag(SyntheticWhileFlag),
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
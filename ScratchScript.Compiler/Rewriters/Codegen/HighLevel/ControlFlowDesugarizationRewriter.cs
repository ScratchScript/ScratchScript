using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.Rewriters.Codegen.HighLevel;

public class ControlFlowDesugarizationRewriter : IrRewriter
{
    public const string ControlFlowCounter = "__CFC";

    public override IrNode VisitForCommand(IrForCommandNode node)
    {
        if (node.Body.Scope is not LoopScope loopScope) throw new Exception();
        loopScope.NextIterationPrerequisite = node.Update;
        return new IrCommandSequenceNode([
            node.Init ?? new IrNoOpCommandNode(),
            new IrWhileCommandNode(node.Condition, node.Body)
        ]);
    }

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node)
    {
        if (CurrentScope is null) throw new Exception();
        if (CurrentScope.GetVariable(ControlFlowCounter) == null)
            CurrentScope.Variables.Add(new ScratchScriptVariable(ControlFlowCounter));

        var times = (IrExpressionNode)Visit(node.Times);
        var body = (IrBlockNode)VisitBlock(node.Body);

        return new IrForCommandNode(
            new IrSetCommandNode(ControlFlowCounter, new IrConstantExpressionNode(TypedValue.Number(0))),
            new IrBinaryExpressionNode(IrBinaryOperator.LessThan,
                new IrLocalVariableIdentifierExpressionNode(ControlFlowCounter), times),
            new IrSetCommandNode(ControlFlowCounter,
                new IrBinaryExpressionNode(IrBinaryOperator.Add,
                    new IrLocalVariableIdentifierExpressionNode(ControlFlowCounter),
                    new IrConstantExpressionNode(TypedValue.Number(1)))), body);
    }
}
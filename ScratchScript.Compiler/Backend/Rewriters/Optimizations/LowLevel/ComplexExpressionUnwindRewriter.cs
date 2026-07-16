using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.Backend.Rewriters.LowLevel;

public class ComplexExpressionUnwindRewriter : IrRewriter
{
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
        if (node.Condition is not IrComplexExpressionNode complexCondition) return node;
        var commands = new List<IrCommandNode>();
        var scope = node.Body.Scope.CloneWithTransformedBody(Visit);
        scope.Body = scope.Body.ConcatNullable(complexCondition.Cleanup).ConcatNullable(complexCondition.Dependencies)
            .ToList();
        commands = commands.ConcatNullable(complexCondition.Dependencies)
            .ConcatNullable(new IrWhileCommandNode(complexCondition.Expression,
                new IrBlockNode(scope))).ConcatNullable(complexCondition.Cleanup).ToList();
        return new IrCommandSequenceNode(commands);
    }

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node)
    {
        return base.VisitRepeatCommand(node);
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        return base.VisitCallFunctionCommand(node);
    }

    public override IrNode VisitIfCommand(IrIfCommandNode node)
    {
        return base.VisitIfCommand(node);
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        if (Visit(node.Expression) is not IrComplexExpressionNode complexValue) return node;
        var commands = new List<IrCommandNode>()
            .ConcatNullable(complexValue.Dependencies)
            .ConcatNullable(new IrSetCommandNode(node.Variable, complexValue.Expression))
            .ConcatNullable(complexValue.Cleanup);
        return new IrCommandSequenceNode(commands);
    }

    public override IrNode VisitRawCommand(IrRawCommandNode node)
    {
        var inputs = new Dictionary<string, IrExpressionNode>(node.Inputs);
        var fields = new Dictionary<string, IrExpressionNode>(node.Fields);

        var dependencies = new List<IrCommandNode>();
        var cleanup = new List<IrCommandNode>();

        foreach (var kvp in node.Fields)
        {
            if (Visit(kvp.Value) is not IrComplexExpressionNode complexValue) continue;
            if (complexValue.Dependencies != null) dependencies.Add(complexValue.Dependencies);
            if (complexValue.Cleanup != null) cleanup.Add(complexValue.Cleanup);
            fields[kvp.Key] = complexValue.Expression;
        }

        foreach (var kvp in node.Inputs)
        {
            if (Visit(kvp.Value) is not IrComplexExpressionNode complexValue) continue;
            if (complexValue.Dependencies != null) dependencies.Add(complexValue.Dependencies);
            if (complexValue.Cleanup != null) cleanup.Add(complexValue.Cleanup);
            inputs[kvp.Key] = complexValue.Expression;
        }

        return new IrCommandSequenceNode(dependencies.ConcatNullable(new IrRawCommandNode(node.Opcode, inputs, fields))
            .ConcatNullable(cleanup));
    }

    public override IrNode VisitPushCommand(IrPushCommand node)
    {
        if (node.Expression is not IrComplexExpressionNode complexExpression) return node;
        var commands = new List<IrCommandNode>()
            .ConcatNullable(complexExpression.Dependencies)
            .ConcatNullable(new IrPushCommand(node.List, complexExpression.Expression))
            .ConcatNullable(complexExpression.Cleanup);
        return new IrCommandSequenceNode(commands);
    }

    public override IrNode VisitPushAtCommand(IrPushAtCommand node)
    {
        return base.VisitPushAtCommand(node);
    }

    public override IrNode VisitPopAtCommand(IrPopAtCommand node)
    {
        return base.VisitPopAtCommand(node);
    }

    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        return base.VisitBinaryExpression(node);
    }
}
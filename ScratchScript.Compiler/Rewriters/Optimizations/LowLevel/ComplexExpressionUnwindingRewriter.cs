using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.AST.Representation.TargetSpecific;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.Rewriters.Optimizations.LowLevel;

public class ComplexExpressionUnwindingRewriter : IrRewriter
{
    private static IrExpressionNode ShiftStackOffset(IrExpressionNode node, int shift)
    {
        if (shift == 0) return node;
        return node switch
        {
            IrStackPointerExpressionNode stp => new IrStackPointerExpressionNode(stp.Offset + shift),
            IrBinaryExpressionNode bin => new IrBinaryExpressionNode(bin.Operator, ShiftStackOffset(bin.Left, shift),
                ShiftStackOffset(bin.Right, shift)),
            IrUnaryExpressionNode un => new IrUnaryExpressionNode(un.Operator, ShiftStackOffset(un.Operand, shift)),
            IrComplexExpressionNode complex => complex with
            {
                Expression = ShiftStackOffset(complex.Expression, shift)
            },
            _ => node
        };
    }

    private static int GetStackWeight(IrExpressionNode node) => node switch
    {
        IrStackPointerExpressionNode stp => stp.Offset + 1,
        IrBinaryExpressionNode bin => Math.Max(GetStackWeight(bin.Left), GetStackWeight(bin.Right)),
        IrUnaryExpressionNode un => GetStackWeight(un.Operand),
        IrComplexExpressionNode complex => GetStackWeight(complex.Expression),
        _ => 0
    };

    private static IrCommandNode? ShiftPopTarget(IrCommandNode? cleanup) => cleanup switch
    {
        IrPopAtCommand popAt => popAt with
        {
            Where = new IrBinaryExpressionNode(IrBinaryOperator.Subtract, popAt.Where,
                new IrConstantExpressionNode(TypedValue.Number(1)))
        },
        IrCommandSequenceNode seq => new IrCommandSequenceNode(
            seq.Commands.Select(ShiftPopTarget).Where(c => c != null)!),
        _ => cleanup
    };

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node) => base.VisitRepeatCommand(node);

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node) =>
        base.VisitCallFunctionCommand(node);

    public override IrNode VisitIfCommand(IrIfCommandNode node)
    {
        if (Visit(node.Condition) is not IrComplexExpressionNode complexCondition) return node;
        var body = (IrBlockNode)Visit(node.Body);
        var alternate = node.Alternate != null ? (IrBlockNode)Visit(node.Alternate) : null;

        return new IrCommandSequenceNode(new List<IrCommandNode>().ConcatNullable(complexCondition.Dependencies)
            .ConcatNullable(new IrIfCommandNode(complexCondition.Expression, body, alternate))
            .ConcatNullable(complexCondition.Cleanup));
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
        var isComplex = false;

        void ShiftPreviousProperties(IrComplexExpressionNode instigator)
        {
            var weight = GetStackWeight(instigator.Expression);
            foreach (var key in inputs.Keys) inputs[key] = ShiftStackOffset(inputs[key], weight);
            foreach (var key in fields.Keys) fields[key] = ShiftStackOffset(fields[key], weight);
        }

        foreach (var kvp in node.Fields)
        {
            if (Visit(kvp.Value) is not IrComplexExpressionNode complexValue) continue;
            ShiftPreviousProperties(complexValue);
            isComplex = true;
            if (complexValue.Dependencies != null) dependencies.Add(complexValue.Dependencies);
            if (complexValue.Cleanup != null) cleanup.Add(complexValue.Cleanup);
            fields[kvp.Key] = complexValue.Expression;
        }

        foreach (var kvp in node.Inputs)
        {
            if (Visit(kvp.Value) is not IrComplexExpressionNode complexValue) continue;
            ShiftPreviousProperties(complexValue);
            isComplex = true;
            if (complexValue.Dependencies != null) dependencies.Add(complexValue.Dependencies);
            if (complexValue.Cleanup != null) cleanup.Add(complexValue.Cleanup);
            inputs[kvp.Key] = complexValue.Expression;
        }

        if (!isComplex) return node;

        return new IrCommandSequenceNode(dependencies.ConcatNullable(new IrRawCommandNode(node.Opcode, inputs, fields))
            .ConcatNullable(cleanup));
    }

    public override IrNode VisitPushCommand(IrPushCommand node)
    {
        if (Visit(node.Expression) is not IrComplexExpressionNode complexExpression) return node;

        var commands = new List<IrCommandNode>()
            .ConcatNullable(complexExpression.Dependencies)
            .ConcatNullable(new IrPushCommand(node.List, complexExpression.Expression))
            .ConcatNullable(ShiftPopTarget(complexExpression.Cleanup));
        return new IrCommandSequenceNode(commands);
    }

    public override IrNode VisitPushAtCommand(IrPushAtCommand node) => base.VisitPushAtCommand(node);

    public override IrNode VisitPopAtCommand(IrPopAtCommand node) => base.VisitPopAtCommand(node);

    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        var left = (IrExpressionNode)Visit(node.Left);
        var right = (IrExpressionNode)Visit(node.Right);
        if (left is not IrComplexExpressionNode && right is not IrComplexExpressionNode) return node;

        var dependencies = new List<IrCommandNode>();
        var cleanup = new List<IrCommandNode>();

        if (left is IrComplexExpressionNode complexLeft)
        {
            if (complexLeft.Dependencies != null) dependencies.Add(complexLeft.Dependencies);
            if (complexLeft.Cleanup != null) cleanup.Add(complexLeft.Cleanup);
            left = complexLeft.Expression;
        }

        if (right is IrComplexExpressionNode complexRight)
        {
            left = ShiftStackOffset(left, GetStackWeight(complexRight.Expression));
            if (complexRight.Dependencies != null) dependencies.Add(complexRight.Dependencies);
            if (complexRight.Cleanup != null) cleanup.Add(complexRight.Cleanup);
            right = complexRight.Expression;
        }

        return new IrComplexExpressionNode(new IrBinaryExpressionNode(node.Operator, left, right),
            new IrCommandSequenceNode(dependencies),
            new IrCommandSequenceNode(cleanup));
    }

    public override IrNode VisitComplexExpression(IrComplexExpressionNode node)
    {
        var result = (IrComplexExpressionNode)base.VisitComplexExpression(node);
        return result.Cleanup == null && result.Dependencies == null ? result.Expression : result;
    }
}
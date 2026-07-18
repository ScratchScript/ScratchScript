using ScratchScript.Compiler.AST.Information;

namespace ScratchScript.Compiler.AST.Representation;

public class IrRewriter : IrBaseVisitor<IrNode>
{
    public override IrNode VisitProgram(IrProgramNode node)
    {
        return node with { Blocks = node.Blocks.Select(b => (IrBlockNode)Visit(b)).ToList() };
    }

    public override IrNode VisitFunction(IrFunctionNode node)
    {
        return node with
        {
            FunctionScope = (FunctionScope)node.FunctionScope.CloneWithTransformedBody(Visit)
        };
    }

    public override IrNode VisitEvent(IrEventNode node)
    {
        return node with { Scope = node.Scope.CloneWithTransformedBody(Visit) };
    }

    public override IrNode VisitCommandSequence(IrCommandSequenceNode node)
    {
        return node with { Commands = node.Commands.Select(c => (IrCommandNode)Visit(c)).ToList() };
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        return node with { Expression = (IrExpressionNode)Visit(node.Expression) };
    }

    public override IrNode VisitWhileCommand(IrWhileCommandNode node)
    {
        return node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            Body = node.Body with { Scope = node.Body.Scope.CloneWithTransformedBody(Visit) }
        };
    }

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node)
    {
        return node with
        {
            Times = (IrExpressionNode)Visit(node.Times),
            Body = node.Body with { Scope = node.Body.Scope.CloneWithTransformedBody(Visit) }
        };
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        return node with
        {
            Arguments = node.Arguments.Select(kvp => (kvp.Item1, (IrExpressionNode)Visit(kvp.Item2))).ToList()
        };
    }

    public override IrNode VisitRawCommand(IrRawCommandNode node)
    {
        return node with
        {
            Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value)),
            Fields = node.Fields.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };
    }

    public override IrNode VisitPushCommand(IrPushCommand node)
    {
        return node with { Expression = (IrExpressionNode)Visit(node.Expression) };
    }

    public override IrNode VisitPushAtCommand(IrPushAtCommand node)
    {
        return node with
        {
            Where = (IrExpressionNode)Visit(node.Where),
            Expression = (IrExpressionNode)Visit(node.Expression)
        };
    }

    public override IrNode VisitPopCommand(IrPopCommand node)
    {
        return node;
    }

    public override IrNode VisitPopAtCommand(IrPopAtCommand node)
    {
        return node with { Where = (IrExpressionNode)Visit(node.Where) };
    }

    public override IrNode VisitPopAllCommand(IrPopAllCommand node)
    {
        return node;
    }

    public override IrNode VisitIfCommand(IrIfCommandNode node)
    {
        return node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            Body = node.Body with { Scope = node.Body.Scope.CloneWithTransformedBody(Visit) },
            Alternate = node.Alternate != null
                ? node.Alternate with { Scope = node.Alternate.Scope.CloneWithTransformedBody(Visit) }
                : null
        };
    }

    public override IrNode VisitConstantExpression(IrConstantExpressionNode node)
    {
        return node;
    }

    public override IrNode VisitGlobalVariableExpression(IrGlobalVariableIdentifierExpressionNode node)
    {
        return node;
    }

    public override IrNode VisitLocalVariableExpression(IrLocalVariableIdentifierExpressionNode node)
    {
        return node;
    }

    public override IrNode VisitGlobalListExpression(IrGlobalListIdentifierExpressionNode node)
    {
        return node;
    }

    public override IrNode VisitParenthesizedExpression(IrParenthesizedExpressionNode node)
    {
        return node with { Expression = (IrExpressionNode)Visit(node.Expression) };
    }

    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        return node with
        {
            Left = (IrExpressionNode)Visit(node.Left),
            Right = (IrExpressionNode)Visit(node.Right)
        };
    }

    public override IrNode VisitUnaryExpression(IrUnaryExpressionNode node)
    {
        return node with { Operand = (IrExpressionNode)Visit(node.Operand) };
    }

    public override IrNode VisitShadowExpression(IrShadowExpressionNode node)
    {
        return node with
        {
            Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value)),
            Fields = node.Fields.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };
    }

    public override IrNode VisitComplexExpression(IrComplexExpressionNode node)
    {
        return node with
        {
            Expression = (IrExpressionNode)Visit(node.Expression),
            Dependencies = node.Dependencies != null ? (IrCommandNode)Visit(node.Dependencies) : null,
            Cleanup = node.Cleanup != null ? (IrCommandNode)Visit(node.Cleanup) : null
        };
    }

    public override IrNode VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node)
    {
        return node with
        {
            Values = node.Values.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };
    }

    public override IrNode VisitTernaryExpression(IrTernaryExpressionNode node)
    {
        return node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            TrueValue = (IrExpressionNode)Visit(node.TrueValue),
            FalseValue = (IrExpressionNode)Visit(node.FalseValue),
        };
    }

    public override IrNode VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
    {
        return node;
    }

    public override IrNode VisitStackPointerExpressionNode(IrStackPointerExpressionNode node)
    {
        return node;
    }

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        return node with
        {
            Arguments = node.Arguments.Select(kvp => (kvp.Item1, (IrExpressionNode)Visit(kvp.Item2))).ToList()
        };
    }

    public override IrNode VisitFunctionReturnCommandNode(IrFunctionReturnCommandNode node)
    {
        return node with { ReturnValue = node.ReturnValue != null ? (IrExpressionNode)Visit(node.ReturnValue) : null };
    }
}
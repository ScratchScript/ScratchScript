using ScratchScript.Compiler.Frontend.Information;

namespace ScratchScript.Compiler.Backend.Representation;

public class IrRewriter : IrBaseVisitor<IrNode>
{
    public override IrNode VisitProgram(IrProgramNode node)
        => node with { Blocks = node.Blocks.Select(b => (IrBlockNode)Visit(b)) };

    public override IrNode VisitFunction(IrFunctionNode node)
        => node with
        {
            FunctionScope = (FunctionScope)node.FunctionScope.CloneWithTransformedBody(Visit)
        };

    public override IrNode VisitEvent(IrEventNode node)
        => node with { Scope = node.Scope.CloneWithTransformedBody(Visit) };

    public override IrNode VisitCommandSequence(IrCommandSequenceNode node)
        => node with { Commands = node.Commands.Select(c => (IrCommandNode)Visit(c)) };

    public override IrNode VisitSetCommand(IrSetCommandNode node)
        => node with { Expression = (IrExpressionNode)Visit(node.Expression) };

    public override IrNode VisitWhileCommand(IrWhileCommandNode node)
        => node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            Body = node.Body with { Scope = node.Body.Scope.CloneWithTransformedBody(Visit) }
        };

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node)
        => node with
        {
            Times = (IrExpressionNode)Visit(node.Times),
            Body = node.Body with { Scope = node.Body.Scope.CloneWithTransformedBody(Visit) }
        };

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
        => node with
        {
            Arguments = node.Arguments.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };

    public override IrNode VisitRawCommand(IrRawCommandNode node)
        => node with
        {
            Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value)),
            Fields = node.Fields.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };

    public override IrNode VisitPushCommand(IrPushCommand node)
        => node with { Expression = (IrExpressionNode)Visit(node.Expression) };

    public override IrNode VisitPushAtCommand(IrPushAtCommand node)
        => node with
        {
            Where = (IrExpressionNode)Visit(node.Where),
            Expression = (IrExpressionNode)Visit(node.Expression)
        };

    public override IrNode VisitPopCommand(IrPopCommand node)
        => node;

    public override IrNode VisitPopAtCommand(IrPopAtCommand node)
        => node with { Where = (IrExpressionNode)Visit(node.Where) };

    public override IrNode VisitPopAllCommand(IrPopAllCommand node)
        => node;

    public override IrNode VisitIfCommand(IrIfCommandNode node)
        => node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            Body = node.Body with { Scope = node.Body.Scope.CloneWithTransformedBody(Visit) },
            Alternate = node.Alternate != null
                ? node.Alternate with { Scope = node.Alternate.Scope.CloneWithTransformedBody(Visit) }
                : null
        };

    public override IrNode VisitConstantExpression(IrConstantExpressionNode node)
        => node;

    public override IrNode VisitGlobalVariableExpression(IrGlobalVariableIdentifierExpressionNode node)
        => node;

    public override IrNode VisitLocalVariableExpression(IrLocalVariableIdentifierExpressionNode node)
        => node;

    public override IrNode VisitGlobalListExpression(IrGlobalListIdentifierExpressionNode node)
        => node;

    public override IrNode VisitParenthesizedExpression(IrParenthesizedExpressionNode node)
        => node with { Expression = (IrExpressionNode)Visit(node.Expression) };

    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
        => node with
        {
            Left = (IrExpressionNode)Visit(node.Left),
            Right = (IrExpressionNode)Visit(node.Right)
        };

    public override IrNode VisitUnaryExpression(IrUnaryExpressionNode node)
        => node with { Operand = (IrExpressionNode)Visit(node.Operand) };

    public override IrNode VisitShadowExpression(IrShadowExpressionNode node)
        => node with
        {
            Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value)),
            Fields = node.Fields.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };

    public override IrNode VisitComplexExpression(IrComplexExpressionNode node)
        => node with
        {
            Expression = (IrExpressionNode)Visit(node.Expression),
            Dependencies = node.Dependencies != null ? (IrCommandNode)Visit(node.Dependencies) : null,
            Cleanup = node.Cleanup != null ? (IrCommandNode)Visit(node.Cleanup) : null
        };

    public override IrNode VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node)
        => node with { Values = node.Values.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value)) };

    public override IrNode VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
        => node;

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
        => node with
        {
            Arguments = node.Arguments.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };
}
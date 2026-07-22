using ScratchScript.Compiler.AST.Information;

namespace ScratchScript.Compiler.AST.Representation;

public class IrRewriter : IrBaseVisitor<IrNode>
{
    protected IrProgramNode ProgramNode { get; set; }
    protected Scope? CurrentScope { get; set; }

    public override IrNode VisitTargetSpecificNode(ITargetSpecificNode node) => node.Rewrite(Visit);

    public override IrNode VisitProgram(IrProgramNode node)
    {
        ProgramNode = node;
        return node with
        {
            Functions = node.Functions.Select(b => (IrFunctionNode)VisitBlock(b)).ToList(),
            Events = node.Events.Select(b => (IrEventNode)VisitBlock(b)).ToList()
        };
    }

    public override IrNode VisitBlock(IrBlockNode node)
    {
        var previousScope = CurrentScope;
        CurrentScope = node.Scope;
        var result = base.VisitBlock(node);
        CurrentScope = previousScope;
        return result;
    }

    public override IrNode VisitFunction(IrFunctionNode node) =>
        node with
        {
            FunctionScope = (FunctionScope)node.FunctionScope.CloneWithTransformedBody(Visit),
            Attributes = node.Attributes?.Select(Visit).OfType<IrAttributeNode>()
        };

    public override IrNode VisitEvent(IrEventNode node) =>
        node with { Scope = node.Scope.CloneWithTransformedBody(Visit) };

    public override IrNode VisitRawBlock(IrBlockNode node) =>
        node with { Scope = node.Scope.CloneWithTransformedBody(Visit) };

    public override IrNode VisitAttribute(IrAttributeNode node) => node with
    {
        Arguments = node.Arguments.Select(Visit).OfType<IrExpressionNode>()
    };

    public override IrNode VisitNoOpCommand(IrNoOpCommandNode node) => node;

    public override IrNode VisitCommandSequence(IrCommandSequenceNode node)
        => node with { Commands = node.Commands.Select(c => (IrCommandNode)Visit(c)).ToList() };

    public override IrNode VisitSetCommand(IrSetCommandNode node) =>
        node with { Expression = (IrExpressionNode)Visit(node.Expression) };

    public override IrNode VisitWhileCommand(IrWhileCommandNode node) =>
        node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            Body = (IrBlockNode)VisitBlock(node.Body)
        };

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node) =>
        node with
        {
            Times = (IrExpressionNode)Visit(node.Times),
            Body = (IrBlockNode)VisitBlock(node.Body)
        };

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node) =>
        node with
        {
            Arguments = node.Arguments.Select(Visit).OfType<IrExpressionNode>()
        };

    public override IrNode VisitRawCommand(IrRawCommandNode node)
    {
        return node with
        {
            Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value)),
            Fields = node.Fields.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };
    }

    public override IrNode VisitPushCommand(IrPushCommand node) =>
        node with { Expression = (IrExpressionNode)Visit(node.Expression) };

    public override IrNode VisitPushAtCommand(IrPushAtCommand node) =>
        node with
        {
            Where = (IrExpressionNode)Visit(node.Where),
            Expression = (IrExpressionNode)Visit(node.Expression)
        };

    public override IrNode VisitPopCommand(IrPopCommand node) => node;

    public override IrNode VisitPopAtCommand(IrPopAtCommand node) =>
        node with { Where = (IrExpressionNode)Visit(node.Where) };

    public override IrNode VisitPopAllCommand(IrPopAllCommand node) => node;

    public override IrNode VisitIfCommand(IrIfCommandNode node) =>
        node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            Body = (IrBlockNode)VisitBlock(node.Body),
            Alternate = node.Alternate != null
                ? (IrBlockNode)VisitBlock(node.Alternate)
                : null
        };

    public override IrNode VisitBreakCommand(IrBreakCommandNode node) => node;

    public override IrNode VisitContinueCommand(IrContinueCommandNode node) => node;

    public override IrNode VisitConstantExpression(IrConstantExpressionNode node) => node;

    public override IrNode VisitGlobalVariableIdentifierExpression(IrGlobalVariableIdentifierExpressionNode node) =>
        node;

    public override IrNode VisitLocalVariableIdentifierExpression(IrLocalVariableIdentifierExpressionNode node) => node;

    public override IrNode VisitGlobalListIdentifierExpression(IrGlobalListIdentifierExpressionNode node) => node;

    public override IrNode VisitParenthesizedExpression(IrParenthesizedExpressionNode node) =>
        node with { Expression = (IrExpressionNode)Visit(node.Expression) };

    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node) =>
        node with
        {
            Left = (IrExpressionNode)Visit(node.Left),
            Right = (IrExpressionNode)Visit(node.Right)
        };

    public override IrNode VisitUnaryExpression(IrUnaryExpressionNode node) =>
        node with { Operand = (IrExpressionNode)Visit(node.Operand) };

    public override IrNode VisitShadowExpression(IrShadowExpressionNode node)
    {
        return node with
        {
            Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value)),
            Fields = node.Fields.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };
    }

    public override IrNode VisitComplexExpression(IrComplexExpressionNode node) =>
        node with
        {
            Expression = (IrExpressionNode)Visit(node.Expression),
            Dependencies = node.Dependencies != null ? (IrCommandNode)Visit(node.Dependencies) : null,
            Cleanup = node.Cleanup != null ? (IrCommandNode)Visit(node.Cleanup) : null
        };

    public override IrNode VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node)
    {
        return node with
        {
            Values = node.Values.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value))
        };
    }

    public override IrNode VisitTernaryExpression(IrTernaryExpressionNode node) =>
        node with
        {
            Condition = (IrExpressionNode)Visit(node.Condition),
            TrueValue = (IrExpressionNode)Visit(node.TrueValue),
            FalseValue = (IrExpressionNode)Visit(node.FalseValue)
        };

    public override IrNode VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node) => node;

    public override IrNode VisitStackPointerExpressionNode(IrStackPointerExpressionNode node) => node;

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node) =>
        node with
        {
            Arguments = node.Arguments.Select(Visit).OfType<IrExpressionNode>()
        };

    public override IrNode VisitFunctionReturnCommandNode(IrReturnCommandNode node) => node with
    {
        ReturnValue = node.ReturnValue != null ? (IrExpressionNode)Visit(node.ReturnValue) : null
    };
}

public static class IrRewriterUtils
{
    public static T RewriteUntilNoChanges<T>(IrRewriter rewriter, T node) where T : IrNode
    {
        var result = node;
        var hash = IrHasher.GetNodeHash(result);
        while (true)
        {
            var nextResult = (T)rewriter.Visit(result);
            var nextHash = IrHasher.GetNodeHash(nextResult);
            if (nextHash == hash) break;
            hash = nextHash;
            result = nextResult;
        }

        return result;
    }

    public static T RewriteUntilNoChanges<T>(Type rewriterType, T node) where T : IrNode =>
        !rewriterType.IsSubclassOf(typeof(IrRewriter))
            ? throw new Exception()
            : RewriteUntilNoChanges((IrRewriter)Activator.CreateInstance(rewriterType)!, node);
}
namespace ScratchScript.Compiler.Backend.Representation;

public abstract class IrBaseVisitor<T>
{
    public virtual T Visit(IrNode node)
    {
        return node switch
        {
            IrBlockNode block => VisitBlock(block),
            IrCommandNode command => VisitCommand(command),
            IrExpressionNode expr => VisitExpression(expr),
            IrProgramNode program => VisitProgram(program),
            _ => throw new NotImplementedException($"No visitor mapping for base node {node.GetType().Name}")
        };
    }

    public virtual T VisitBlock(IrBlockNode node)
    {
        return node switch
        {
            IrFunctionNode func => VisitFunction(func),
            IrEventNode ev => VisitEvent(ev),
            _ => throw new NotImplementedException($"No visitor mapping for block {node.GetType().Name}")
        };
    }

    public virtual T VisitCommand(IrCommandNode node)
    {
        return node switch
        {
            IrCommandSequenceNode seq => VisitCommandSequence(seq),
            IrSetCommandNode set => VisitSetCommand(set),
            IrWhileCommandNode wh => VisitWhileCommand(wh),
            IrRepeatCommandNode rep => VisitRepeatCommand(rep),
            IrCallFunctionCommandNode c => VisitCallFunctionCommand(c),
            IrRawCommandNode raw => VisitRawCommand(raw),
            IrPushCommand push => VisitPushCommand(push),
            IrPushAtCommand pushAt => VisitPushAtCommand(pushAt),
            IrPopCommand pop => VisitPopCommand(pop),
            IrPopAtCommand popAt => VisitPopAtCommand(popAt),
            IrPopAllCommand popAll => VisitPopAllCommand(popAll),
            IrIfCommandNode ifs => VisitIfCommand(ifs),
            IrFunctionReturnCommandNode fret => VisitFunctionReturnCommandNode(fret),
            _ => throw new NotImplementedException($"No visitor mapping for command {node.GetType().Name}")
        };
    }

    public virtual T VisitExpression(IrExpressionNode node)
    {
        return node switch
        {
            IrConstantExpressionNode cons => VisitConstantExpression(cons),
            IrLocalVariableIdentifierExpressionNode lvar => VisitLocalVariableExpression(lvar),
            IrGlobalVariableIdentifierExpressionNode var => VisitGlobalVariableExpression(var),
            IrGlobalListIdentifierExpressionNode list => VisitGlobalListExpression(list),
            IrParenthesizedExpressionNode paren => VisitParenthesizedExpression(paren),
            IrBinaryExpressionNode bin => VisitBinaryExpression(bin),
            IrUnaryExpressionNode un => VisitUnaryExpression(un),
            IrShadowExpressionNode shadow => VisitShadowExpression(shadow),
            IrFunctionArgumentExpressionNode farg => VisitFunctionArgumentExpressionNode(farg),
            IrFunctionCallExpressionNode fcall => VisitFunctionCallExpressionNode(fcall),
            IrComplexExpressionNode complex => VisitComplexExpression(complex),
            IrObjectLiteralExpressionNode obj => VisitObjectLiteralExpression(obj),
            IrStackPointerExpressionNode stp => VisitStackPointerExpressionNode(stp),
            IrTernaryExpressionNode ternary => VisitTernaryExpression(ternary),
            _ => throw new NotImplementedException($"No visitor mapping for expression {node.GetType().Name}")
        };
    }

    public abstract T VisitProgram(IrProgramNode node);
    public abstract T VisitFunction(IrFunctionNode node);
    public abstract T VisitEvent(IrEventNode node);
    public abstract T VisitCommandSequence(IrCommandSequenceNode node);
    public abstract T VisitSetCommand(IrSetCommandNode node);
    public abstract T VisitWhileCommand(IrWhileCommandNode node);
    public abstract T VisitRepeatCommand(IrRepeatCommandNode node);
    public abstract T VisitCallFunctionCommand(IrCallFunctionCommandNode node);
    public abstract T VisitRawCommand(IrRawCommandNode node);
    public abstract T VisitPushCommand(IrPushCommand node);
    public abstract T VisitPushAtCommand(IrPushAtCommand node);
    public abstract T VisitPopCommand(IrPopCommand node);
    public abstract T VisitPopAtCommand(IrPopAtCommand node);
    public abstract T VisitPopAllCommand(IrPopAllCommand node);
    public abstract T VisitIfCommand(IrIfCommandNode node);
    public abstract T VisitConstantExpression(IrConstantExpressionNode node);
    public abstract T VisitGlobalVariableExpression(IrGlobalVariableIdentifierExpressionNode node);
    public abstract T VisitLocalVariableExpression(IrLocalVariableIdentifierExpressionNode node);
    public abstract T VisitGlobalListExpression(IrGlobalListIdentifierExpressionNode node);
    public abstract T VisitParenthesizedExpression(IrParenthesizedExpressionNode node);
    public abstract T VisitBinaryExpression(IrBinaryExpressionNode node);
    public abstract T VisitUnaryExpression(IrUnaryExpressionNode node);
    public abstract T VisitShadowExpression(IrShadowExpressionNode node);
    public abstract T VisitComplexExpression(IrComplexExpressionNode node);
    public abstract T VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node);
    public abstract T VisitTernaryExpression(IrTernaryExpressionNode node);
    public abstract T VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node);
    public abstract T VisitStackPointerExpressionNode(IrStackPointerExpressionNode node);
    public abstract T VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node);
    public abstract T VisitFunctionReturnCommandNode(IrFunctionReturnCommandNode node);
}
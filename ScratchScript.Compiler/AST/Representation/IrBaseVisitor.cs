namespace ScratchScript.Compiler.AST.Representation;

public abstract class IrBaseVisitor<T>
{
    public virtual T Visit(IrNode node)
    {
        return node switch
        {
            ITargetSpecificNode targetSpecific => VisitTargetSpecificNode(targetSpecific),
            IrBlockNode block => VisitBlock(block),
            IrCommandNode command => VisitCommand(command),
            IrExpressionNode expr => VisitExpression(expr),
            IrProgramNode program => VisitProgram(program),
            IrAttributeNode attribute => VisitAttribute(attribute),
            _ => throw new NotImplementedException($"No visitor mapping for base node {node.GetType().Name}")
        };
    }

    public virtual T VisitBlock(IrBlockNode node)
    {
        return node switch
        {
            IrFunctionNode func => VisitFunction(func),
            IrEventNode ev => VisitEvent(ev),
            _ => VisitRawBlock(node)
        };
    }

    public virtual T VisitCommand(IrCommandNode node)
    {
        return node switch
        {
            IrNoOpCommandNode noop => VisitNoOpCommand(noop),
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
            IrReturnCommandNode fret => VisitFunctionReturnCommandNode(fret),
            IrBreakCommandNode br => VisitBreakCommand(br),
            IrContinueCommandNode cont => VisitContinueCommand(cont),
            _ => throw new NotImplementedException($"No visitor mapping for command {node.GetType().Name}")
        };
    }

    public virtual T VisitExpression(IrExpressionNode node)
    {
        return node switch
        {
            IrConstantExpressionNode cons => VisitConstantExpression(cons),
            IrLocalVariableIdentifierExpressionNode lvar => VisitLocalVariableIdentifierExpression(lvar),
            IrGlobalVariableIdentifierExpressionNode var => VisitGlobalVariableIdentifierExpression(var),
            IrGlobalListIdentifierExpressionNode list => VisitGlobalListIdentifierExpression(list),
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

    public abstract T VisitTargetSpecificNode(ITargetSpecificNode node);
    public abstract T VisitProgram(IrProgramNode node);
    public abstract T VisitFunction(IrFunctionNode node);
    public abstract T VisitEvent(IrEventNode node);
    public abstract T VisitRawBlock(IrBlockNode node);
    public abstract T VisitAttribute(IrAttributeNode node);
    public abstract T VisitNoOpCommand(IrNoOpCommandNode node);
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
    public abstract T VisitBreakCommand(IrBreakCommandNode node);
    public abstract T VisitContinueCommand(IrContinueCommandNode node);
    public abstract T VisitConstantExpression(IrConstantExpressionNode node);
    public abstract T VisitGlobalVariableIdentifierExpression(IrGlobalVariableIdentifierExpressionNode node);
    public abstract T VisitLocalVariableIdentifierExpression(IrLocalVariableIdentifierExpressionNode node);
    public abstract T VisitGlobalListIdentifierExpression(IrGlobalListIdentifierExpressionNode node);
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
    public abstract T VisitFunctionReturnCommandNode(IrReturnCommandNode node);
}
using ScratchScript.Compiler.AST.Representation;

namespace ScratchScript.Compiler.ProjectEmitter;

public partial class ScratchScriptProjectEmitter
{
    public override object VisitTargetSpecificNode(ITargetSpecificNode node) => throw new NotImplementedException();
    public override object VisitImport(IrImportNode node) => throw new NotImplementedException();
    public override object VisitEnum(IrEnumNode node) => throw new NotImplementedException();

    public override object VisitForCommand(IrForCommandNode node) => throw new NotImplementedException();

    public override object VisitBreakCommand(IrBreakCommandNode node) => throw new NotImplementedException();

    public override object VisitContinueCommand(IrContinueCommandNode node) => throw new NotImplementedException();

    public override object VisitMemberCallFunctionCommand(IrMemberCallFunctionCommandNode node) =>
        throw new NotImplementedException();

    public override object VisitLocalVariableIdentifierExpression(IrLocalVariableIdentifierExpressionNode node) =>
        throw new NotImplementedException();

    public override object VisitGlobalListIdentifierExpression(IrGlobalListIdentifierExpressionNode node) =>
        throw new NotImplementedException();

    public override object VisitComplexExpression(IrComplexExpressionNode node) => throw new NotImplementedException();

    public override object VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node) =>
        throw new NotImplementedException();

    public override object VisitTernaryExpression(IrTernaryExpressionNode node) => throw new NotImplementedException();

    public override object VisitStackPointerExpression(IrStackPointerExpressionNode node) =>
        throw new NotImplementedException();

    public override object VisitTypeReferenceExpression(IrTypeReferenceExpressionNode node) =>
        throw new NotImplementedException();

    public override object VisitMemberPropertyExpression(IrMemberPropertyExpressionNode node) =>
        throw new NotImplementedException();

    public override object VisitMemberFunctionCallExpression(IrMemberFunctionCallExpressionNode node) =>
        throw new NotImplementedException();
}
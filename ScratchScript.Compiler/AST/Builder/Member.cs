using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor
{
    public override IrNode? VisitMemberPropertyAccessExpression(
        ScratchScriptParser.MemberPropertyAccessExpressionContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode expression)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }
        var identifier = context.Identifier().GetText();
        return new IrMemberPropertyExpressionNode(expression, identifier);
    }
}
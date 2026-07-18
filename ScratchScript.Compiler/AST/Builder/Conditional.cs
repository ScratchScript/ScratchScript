using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor
{
    public override IrNode? VisitWhileStatement(ScratchScriptParser.WhileStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode condition)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        /*if (DetermineExpressionType(condition) != ScratchType.Boolean)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(condition));
            return null;
        }*/

        if (Visit(context.lineOrBlock()) is not IrBlockNode body)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock());
            return null;
        }

        return new IrWhileCommandNode(condition, body).WithContext(context);
    }

    public override IrNode? VisitRepeatStatement(ScratchScriptParser.RepeatStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode times)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        /*if (DetermineExpressionType(times) != ScratchType.Boolean)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(times));
            return null;
        }*/

        if (Visit(context.lineOrBlock()) is not IrBlockNode body)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock());
            return null;
        }

        return new IrRepeatCommandNode(times, body).WithContext(context);
    }

    public override IrNode? VisitIfStatement(ScratchScriptParser.IfStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode condition)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        /*if (DetermineExpressionType(condition) != ScratchType.Boolean)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(condition));
            return null;
        }*/

        var block = (IrBlockNode)Visit(context.lineOrBlock(0))!;
        if (context.lineOrBlock(1) == null) return new IrIfCommandNode(condition, block, null).WithContext(context);

        if (Visit(context.lineOrBlock(1)) is not IrBlockNode alternate)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock(1));
            return null;
        }

        return new IrIfCommandNode(condition, block, alternate).WithContext(context);
    }

    public override IrNode? VisitLineOrBlock(ScratchScriptParser.LineOrBlockContext context)
    {
        if (context.block() != null && Visit(context.block()) is IrBlockNode blockNode) return blockNode;
        if (context.line() != null && Visit(context.line()) is IrCommandNode lineNode)
            return new IrBlockNode([lineNode]).WithContext(context.line());
        return null;
    }
}
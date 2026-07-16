using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override IrNode? VisitWhileStatement(ScratchScriptParser.WhileStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode condition)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        if (DetermineExpressionType(condition) != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(condition));
            return null;
        }

        if (Visit(context.lineOrBlock()) is not IrBlockNode body)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock());
            return null;
        }

        return new IrWhileCommandNode(condition, body);
    }

    public override IrNode? VisitRepeatStatement(ScratchScriptParser.RepeatStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode times)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        if (DetermineExpressionType(times) != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(times));
            return null;
        }

        if (Visit(context.lineOrBlock()) is not IrBlockNode body)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock());
            return null;
        }

        return new IrRepeatCommandNode(times, body);
    }

    public override IrNode? VisitIfStatement(ScratchScriptParser.IfStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode condition)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        if (DetermineExpressionType(condition) != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(condition));
            return null;
        }

        var block = (IrBlockNode)VisitBlock(context.block())!;
        if (context.lineOrBlock() == null) return new IrIfCommandNode(condition, block, null);

        if (Visit(context.lineOrBlock()) is not IrBlockNode alternate)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock());
            return null;
        }

        return new IrIfCommandNode(condition, block, alternate);
    }

    public override IrNode? VisitLineOrBlock(ScratchScriptParser.LineOrBlockContext context)
    {
        if (context.block() != null && Visit(context.block()) is IrBlockNode blockNode) return blockNode;
        if (context.line() != null && Visit(context.line()) is IrCommandNode lineNode)
            return new IrBlockNode([lineNode]);
        return null;
    }
}
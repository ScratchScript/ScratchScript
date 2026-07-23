using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor
{
    public override IrNode? VisitWhileStatement(ScratchScriptParser.WhileStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode condition)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context,
                context.expression());
            return null;
        }

        /*if (DetermineExpressionType(condition) != ScratchType.Boolean)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(condition));
            return null;
        }*/

        if (VisitLineOrBlock(context.lineOrBlock(), new LoopScope { Kind = LoopScopeKind.While }) is not { } body)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock());
            return null;
        }

        return new IrWhileCommandNode(condition, body).WithContext(context);
    }

    public override IrNode? VisitForStatement(ScratchScriptParser.ForStatementContext context)
    {
        IrCommandNode? init = null;
        IrExpressionNode condition;
        IrCommandNode? update = null;

        if (context.statement(0) != null)
        {
            if (Visit(context.statement(0)) is not IrCommandNode rawInit)
            {
                DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedStatement, context,
                    context.statement(0));
                return null;
            }

            init = rawInit;
        }

        if (context.expression() != null)
        {
            if (Visit(context.expression()) is not IrExpressionNode rawCondition)
            {
                DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context,
                    context.expression());
                return null;
            }

            condition = rawCondition;
        }
        else
            condition = new IrBinaryExpressionNode(IrBinaryOperator.Equal,
                new IrConstantExpressionNode(TypedValue.Number(1)), new IrConstantExpressionNode(TypedValue.Number(1)));

        if (VisitLineOrBlock(context.lineOrBlock(), new LoopScope { Kind = LoopScopeKind.For }) is not { } body)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.lineOrBlock());
            return null;
        }

        if (context.statement(1) != null)
        {
            var rawUpdate = VisitInScope(body.Scope, () =>
            {
                if (Visit(context.statement(1)) is IrCommandNode rawUpdate) return rawUpdate;
                DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedStatement, context,
                    context.statement(1));
                return null;
            });
            if (rawUpdate == null) return null;
            update = rawUpdate;
        }

        if (body.Scope is LoopScope loopScope) loopScope.NextIterationPrerequisite = update;
        return new IrCommandSequenceNode([
            init ?? new IrNoOpCommandNode(),
            new IrWhileCommandNode(condition, body)
        ]);
    }

    public override IrNode? VisitRepeatStatement(ScratchScriptParser.RepeatStatementContext context)
    {
        if (Visit(context.expression()) is not IrExpressionNode times)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context,
                context.expression());
            return null;
        }

        /*if (DetermineExpressionType(times) != ScratchType.Boolean)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, DetermineExpressionType(times));
            return null;
        }*/

        if (VisitLineOrBlock(context.lineOrBlock(), new LoopScope { Kind = LoopScopeKind.Repeat }) is not { } body)
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
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context,
                context.expression());
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

    public override IrNode? VisitLineOrBlock(ScratchScriptParser.LineOrBlockContext context) =>
        VisitLineOrBlock(context);

    private IrBlockNode? VisitLineOrBlock(ScratchScriptParser.LineOrBlockContext context, Scope? intoScope = null)
    {
        var scope = intoScope ?? new Scope();
        if (context.block() != null && VisitBlock(scope, context.block()) is { } blockNode)
            return blockNode;
        if (context.line() != null && Visit(context.line()) is IrCommandNode lineNode)
        {
            scope.ParentScope = _scope;
            scope.Body.Add(lineNode);
            return new IrBlockNode(scope).WithContext(context.line());
        }

        return null;
    }

    public override IrNode? VisitBreakStatement(ScratchScriptParser.BreakStatementContext context)
    {
        if (_scope is LoopScope loop) loop.HasBreak = true;
        return new IrBreakCommandNode().WithContext(context);
    }

    public override IrNode? VisitContinueStatement(ScratchScriptParser.ContinueStatementContext context)
    {
        if (_scope is LoopScope loop) loop.HasContinue = true;
        return new IrContinueCommandNode().WithContext(context);
    }

    private T VisitInScope<T>(Scope scope, Func<T> visit)
    {
        var previousScope = _scope;
        _scope = scope;
        var result = visit();
        _scope = previousScope;
        return result;
    }

    private void VisitInScope(Scope scope, Action visit)
    {
        var previousScope = _scope;
        _scope = scope;
        visit();
        _scope = previousScope;
    }
}
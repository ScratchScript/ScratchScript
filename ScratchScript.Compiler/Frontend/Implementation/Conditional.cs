using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private IConditionalHandler _conditionalHandler = null!;

    public override TypedValue? VisitForStatement(ScratchScriptParser.ForStatementContext context)
    {
        /*
         *  for loop structure:
         *
         *  {initializer dependencies}
         *  {initializer body}
         *  {condition dependencies}
         *  while(condition) {
         *       {body}
         *       {change dependencies}
         *       {change body} <- must be processed INSIDE the scope via VisitInScope
         *       {change cleanup} <- not outside the loop
         *       {condition cleanup}
         *       {condition dependencies} <- because it needs to be recalculated
         *  }
         *  {condition cleanup}
         *  {initializer cleanup}
         */

        StatementValue? initialize = null;
        ExpressionValue? condition = null;
        StatementValue? change = null;

        // initializer
        if (context.statement(0) != null)
        {
            if (Visit(context.statement(0)) is not StatementValue initializeStatement)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedStatement, context, context.statement(0));
                return null;
            }

            initialize = initializeStatement;
        }

        // condition
        if (context.expression() != null)
        {
            if (Visit(context.expression()) is not ExpressionValue conditionExpression)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
                return null;
            }

            condition = conditionExpression;
        }
        else
        {
            condition = new ExpressionValue("== 1 1",
                ScratchType.Boolean); // TODO: temporary implementation, good for a "while true" condition
        }

        condition = _conditionalHandler.GetEqualityExpression(condition);

        // condition must be a boolean
        if (condition.Type != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, condition.Type);
            return null;
        }

        // put all statements into a new scope
        var scope = CreateDefaultScope();
        scope.Header =
        [
            ..initialize?.Dependencies ?? [], ..initialize?.Commands ?? [], ..condition.Dependencies ?? [],
            $"while {condition.Value}"
        ];
        scope.End = ["end", ..condition.Cleanup ?? [], ..initialize?.Cleanup ?? []];
        scope = VisitBlock(scope, context.block()).Scope;

        // get the change statement inside the scope because otherwise
        // it messes with the TotalIntermediateStackCount
        var changeFailed = VisitInScope(scope, () =>
        {
            if (context.statement(1) != null)
            {
                if (Visit(context.statement(1)) is not StatementValue changeStatement)
                {
                    DiagnosticReporter.Error((int)ScratchScriptError.ExpectedStatement, context, context.statement(1));
                    return true;
                }

                change = changeStatement;
            }

            return false;
        });
        if (changeFailed) return null;

        scope.Content.AddRange([
            ..change?.Dependencies ?? [], ..change?.Commands ?? [], ..change?.Cleanup ?? [], ..condition.Cleanup ?? [],
            ..condition.Dependencies ?? []
        ]);

        return new ScopeValue(scope);
    }

    public override TypedValue? VisitRepeatStatement(ScratchScriptParser.RepeatStatementContext context)
    {
        // ICE handling
        if (Visit(context.expression()) is not ExpressionValue times)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        // repetition amount should be a number
        if (times.Type != ScratchType.Number)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, times.Type);
            return null;
        }

        // put all statements into a new scope
        var scope = CreateDefaultScope();
        scope.Header = [..times.Dependencies ?? [], $"repeat {times.Value}"];
        scope.End = ["end", ..times.Cleanup ?? []];
        scope = VisitBlock(scope, context.block()).Scope;

        return new ScopeValue(scope);
    }

    public override TypedValue? VisitWhileStatement(ScratchScriptParser.WhileStatementContext context)
    {
        /*
         *  while loop structure:
         *
         *  {condition dependencies}
         *  while(condition) {
         *       {body}
         *       {condition cleanup}
         *       {condition dependencies} <- because it needs to be recalculated
         *  }
         *  {condition cleanup} <- if the loop exits, the leftover data is still there,
         *                         so it needs to be cleaned
         */

        // ICE handling
        if (Visit(context.expression()) is not ExpressionValue condition)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        // condition must be a boolean
        if (condition.Type != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, condition.Type);
            return null;
        }

        // the "X is true" expression is target-specific and can be optimized via extensions
        var conditionExpression = _conditionalHandler.GetEqualityExpression(condition);
        if (conditionExpression.Value == null)
            throw new Exception("The IConditionHandler returned an invalid expression for GetEqualityExpression.");

        // put all statements into a new scope
        var scope = CreateDefaultScope();
        scope.Header = [..condition.Dependencies ?? [], $"while {conditionExpression.Value}"];
        scope.End = ["end", ..condition.Cleanup ?? []];
        scope = VisitBlock(scope, context.block()).Scope;
        scope.Content.AddRange([..condition.Cleanup ?? [], ..condition.Dependencies ?? []]);

        return new ScopeValue(scope);
    }

    public override TypedValue? VisitIfStatement(ScratchScriptParser.IfStatementContext context)
    {
        // ICE handling
        if (Visit(context.expression()) is not ExpressionValue condition)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        // condition must be a boolean
        if (condition.Type != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, condition.Type);
            return null;
        }

        // the "X is true" expression is target-specific and can be optimized via extensions
        var conditionExpression = _conditionalHandler.GetEqualityExpression(condition);
        if (conditionExpression.Value == null)
            throw new Exception("The IConditionHandler returned an invalid expression for GetEqualityExpression.");

        // put all statements into a new scope
        var scope = CreateDefaultScope();
        scope.Header = [..condition.Dependencies ?? [], $"if {conditionExpression.Value}"];
        scope.End = ["end", ..condition.Cleanup ?? []];
        scope = VisitBlock(scope, context.block()).Scope;

        // put the else-clause immediately after the if-clause (not the same scope!)
        if (context.elseIfStatement() != null &&
            VisitElseIfStatement(context.elseIfStatement()) is ScopeValue elseScopeValue)
        {
            scope.End.Remove("end");
            scope.Content.Add("end");
            scope.Content.Add(elseScopeValue.Scope.ToString(Settings.CommandSeparator));
        }

        return new ScopeValue(scope);
    }

    public override TypedValue? VisitElseIfStatement(ScratchScriptParser.ElseIfStatementContext context)
    {
        var scope = CreateDefaultScope();
        scope.Header = ["else"];
        scope.ParentScope = _scope;

        if (context.ifStatement() != null)
        {
            if (VisitIfStatement(context.ifStatement()) is not ScopeValue ifScope)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.ifStatement());
                return null;
            }

            scope.Content.Add(ifScope.Scope.ToString(Settings.CommandSeparator));
        }
        else if (context.block() != null)
        {
            scope = VisitBlock(scope, context.block()).Scope;
        }

        return new ScopeValue(scope);
    }

    private T VisitInScope<T>(IScope scope, Func<T> visit)
    {
        var lastScope = _scope;
        _scope = scope;
        var result = visit();
        _scope = lastScope;
        return result;
    }

    private void VisitInScope(IScope scope, Action visit)
    {
        var lastScope = _scope;
        _scope = scope;
        visit();
        _scope = lastScope;
    }
}
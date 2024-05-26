using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private IConditionalHandler _conditionalHandler = null!;

    public override TypedValue? VisitIfStatement(ScratchScriptParser.IfStatementContext context)
    {
        // ICE handling
        if (Visit(context.expression()) is not ExpressionValue condition)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
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
        _scope = scope;
        
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
            scope = VisitBlock(scope, context.block()).Scope;

        _scope = scope.ParentScope;
        return new ScopeValue(scope);
    }
}
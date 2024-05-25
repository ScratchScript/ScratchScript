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
        scope.Header = [..condition.Dependencies ?? [], $"if {conditionExpression.Value}", ..condition.Cleanup ?? []];
        scope = VisitBlock(scope, context.block()).Scope;

        // put the else-clause immediately after the if-clause (not the same scope!)
        if (context.elseIfStatement() != null &&
            VisitElseIfStatement(context.elseIfStatement()) is ScopeValue elseScopeValue)
        {
            scope.Content.AddRange(["end", "else", ..condition.Cleanup ?? []]);
            scope.Content.Add(elseScopeValue.Scope.ToString(Settings.CommandSeparator));
        }

        return new ScopeValue(scope);
    }

    public override TypedValue? VisitElseIfStatement(ScratchScriptParser.ElseIfStatementContext context)
    {
        if (context.ifStatement() != null) return VisitIfStatement(context.ifStatement());
        if (context.block() != null)
        {
            var scope = CreateDefaultScope();
            scope.Header = ["else"];
            return VisitBlock(scope, context.block());
        }

        return null;
    }
}
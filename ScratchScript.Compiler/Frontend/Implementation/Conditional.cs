using System.Net.Mime;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitIfStatement(ScratchScriptParser.IfStatementContext context)
    {
        if (Visit(context.expression()) is not ExpressionValue condition)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        if (condition.Type != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(), ScratchType.Boolean, condition.Type);
            return null;
        }

        var scope = CreateDefaultScope();
        // TODO: get rid of equal to "true" check later
        scope.Header = [..condition.Dependencies ?? [], $"if == {condition.Value} \"true\""];
        scope = VisitBlock(scope, context.block()).Scope;

        if (context.elseIfStatement() != null &&
            VisitElseIfStatement(context.elseIfStatement()) is ScopeValue elseScopeValue)
        {
            var elseScope = elseScopeValue.Scope;
            if (context.elseIfStatement().ifStatement() != null)
                elseScope.Header[0] = $"else {elseScope.Header[0]}";
         
            scope.Content.Add(elseScope.ToString(Settings.CommandSeparator));
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
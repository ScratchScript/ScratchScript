using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitVariableDeclarationStatement(
        ScratchScriptParser.VariableDeclarationStatementContext context)
    {
        var name = context.Identifier().GetText();

        // check if the name is available
        if (RequireIdentifierUnclaimedOrFail(name, ownContext: context, ownIdentifier: context.Identifier()))
            return null;

        // in case of an ICE
        if (Visit(context.expression()) is not ExpressionValue expression)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        if (Scope == null) throw new Exception("Cannot declare variables without a scope.");
        Scope.AddVariable(name, GenerateVariableId(name), expression);

        if (!LocationInformation.Variables.ContainsKey(Scope.Depth)) // since it's a nested dictionary
            LocationInformation.Variables[Scope.Depth] = new Dictionary<string, VariableLocationInformation>();
        LocationInformation.Variables[Scope.Depth][name] = new VariableLocationInformation
        {
            Context = context,
            Identifier = context.Identifier(),
            TypeSetterExpression = context.expression()
        };
        return null;
    }

    public override TypedValue? VisitAssignmentStatement(ScratchScriptParser.AssignmentStatementContext context)
    {
        var name = context.Identifier().GetText();

        if (Scope?.GetVariable(name) is not { } variable)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.VariableNotDefined, context, context.Identifier(), name);
            return null;
        }

        // in case of an ICE
        if (Visit(context.expression()) is not ExpressionValue expression)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        if (variable.Type != expression.Type)
        {
            var locationInformation = LocationInformation.Variables[Scope.Depth][name];

            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(), variable.Type,
                expression.Type);
            DiagnosticReporter.Note((int)ScratchScriptNote.VariableTypeSetAt, locationInformation.Context,
                locationInformation.TypeSetterExpression);
            return null;
        }
        
        Scope.SetVariable(variable, expression);
        return null;
    }

    // TODO: think if this can be improved (and also not just random characters)
    private string GenerateVariableId(string name) => $"_{Id[..5]}_{Scope?.Depth}_{name}";
}
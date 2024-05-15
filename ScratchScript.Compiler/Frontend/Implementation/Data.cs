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
        if (RequireIdentifierUnclaimedOrFail(name, context, context.Identifier()))
            return null;

        // in case of an ICE
        if (Visit(context.expression()) is not ExpressionValue expression)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        if (_scope == null) throw new Exception("Cannot declare variables without a scope.");
        _dataHandler.AddVariable(ref _scope, name, _dataHandler.GenerateVariableId(_scope.Depth, Id, name), expression);

        if (!LocationInformation.Variables.ContainsKey(_scope.Depth)) // since it's a nested dictionary
            LocationInformation.Variables[_scope.Depth] = new Dictionary<string, VariableLocationInformation>();
        LocationInformation.Variables[_scope.Depth][name] = new VariableLocationInformation
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

        if (_scope?.GetVariable(name) is not { } variable)
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
            var locationInformation = LocationInformation.Variables[_scope.Depth][name];

            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(), variable.Type,
                expression.Type);
            DiagnosticReporter.Note((int)ScratchScriptNote.VariableTypeSetAt, locationInformation.Context,
                locationInformation.TypeSetterExpression);
            return null;
        }

        _dataHandler.SetVariable(ref _scope, variable, expression);
        return null;
    }
}
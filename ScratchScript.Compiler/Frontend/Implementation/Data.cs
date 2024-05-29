﻿using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private IDataHandler _dataHandler = null!;

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
        var statement = _dataHandler.AddVariable(ref _scope, name,
            _dataHandler.GenerateVariableId(_scope.Depth, Id, name), expression);

        if (!LocationInformation.Variables.ContainsKey(_scope.Depth)) // since it's a nested dictionary
            LocationInformation.Variables[_scope.Depth] = new Dictionary<string, VariableLocationInformation>();
        LocationInformation.Variables[_scope.Depth][name] = new VariableLocationInformation
        {
            Context = context,
            Identifier = context.Identifier(),
            TypeSetterExpression = context.expression()
        };
        return statement;
    }

    public override TypedValue? VisitAssignmentStatement(ScratchScriptParser.AssignmentStatementContext context)
    {
        var name = context.Identifier().GetText();

        // in case of an ICE
        if (Visit(context.expression()) is not ExpressionValue expression)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        if (_scope is IFunctionScope function && function.Arguments.FirstOrDefault(arg => arg.Name == name) is
                { } argument)
        {
            if (argument.Type != expression.Type)
            {
                var typeSetter = LocationInformation.Functions[function.FunctionName].ArgumentInformation[name]
                    .TypeSetter;
                if (typeSetter == null)
                    throw new Exception(
                        $"DiagnosticLocationStorage didn't contain the type setter for function argument \"{name}\" (function \"{function.FunctionName}\")");

                DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                    argument.Type, expression.Type);
                DiagnosticReporter.Note((int)ScratchScriptNote.FunctionArgumentTypeSetAt, typeSetter, typeSetter);
                return null;
            }

            _functionHandler.HandleFunctionArgumentAssignment(ref _scope, name, expression);
            return null;
        }

        if (_scope.GetVariable(name) is not { } variable)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.VariableNotDefined, context, context.Identifier(), name);
            return null;
        }

        //NOTE: MustMatchTypesOrFail cannot be used here
        if (variable.Type != expression.Type)
        {
            var locationInformation = LocationInformation.Variables[_scope.Depth][name];

            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(), variable.Type,
                expression.Type);
            DiagnosticReporter.Note((int)ScratchScriptNote.VariableTypeSetAt, locationInformation.Context,
                locationInformation.TypeSetterExpression);
            return null;
        }

        return _dataHandler.SetVariable(ref _scope, variable, expression);
        ;
    }
}
using ScratchScript.Compiler.Diagnostics;
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
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        if (_scope == null) throw new Exception("Cannot declare variables without a scope.");
        var statement = _dataHandler.AddVariable(_scope, name,
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
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        if (Visit(context.assignmentOperators()) is not GenericValue<AssignmentOperator> assignmentOperator)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.assignmentOperators());
            return null;
        }

        if (_scope is IFunctionScope function && function.Arguments.FirstOrDefault(arg => arg.Name == name) is
                { } argument)
        {
            if (function.Inlined)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.CannotAssignFunctionArgumentInInlinedFunction, context,
                    context);
                return null;
            }

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

            expression = ConvertAssignmentToBinaryExpression(assignmentOperator.Value,
                (ExpressionValue)_functionHandler.GetArgument(_scope, name), expression);
            return _functionHandler.HandleFunctionArgumentAssignment(_scope, name, expression);
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

        expression = ConvertAssignmentToBinaryExpression(assignmentOperator.Value,
            (ExpressionValue)_dataHandler.GetVariable(_scope, variable), expression);
        return _dataHandler.SetVariable(_scope, variable, expression);
    }

    // TODO: add bitwise operations support when imports are implemented
    private ExpressionValue ConvertAssignmentToBinaryExpression(AssignmentOperator op, ExpressionValue variable,
        ExpressionValue value)
    {
        if (_scope is null) throw new Exception("Cannot perform variable assignment in the root scope.");

        switch (op)
        {
            case AssignmentOperator.Assignment:
                return value;
            case AssignmentOperator.AdditionAssignment:
            case AssignmentOperator.SubtractionAssignment:
            {
                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                var str = op switch
                {
                    AssignmentOperator.AdditionAssignment when variable.Type.Kind is ScratchTypeKind.String &&
                                                               value.Type.Kind is ScratchTypeKind.String =>
                        $"~ {variable.Value} {value.Value}",
                    AssignmentOperator.AdditionAssignment => $"+ {variable.Value} {value.Value}",
                    AssignmentOperator.SubtractionAssignment => $"- {variable.Value} {value.Value}",
                    _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
                };
                return new ExpressionValue(str, variable.Type, value.Dependencies, value.Cleanup);
            }
            case AssignmentOperator.MultiplicationAssignment:
            case AssignmentOperator.DivisionAssignment:
            case AssignmentOperator.ModulusAssignment:
            case AssignmentOperator.PowerAssignment:
                return _binaryHandler.GetBinaryMultiplyExpression(_scope, op.ToMultiplyOperator(), variable, value);
            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
    }
}
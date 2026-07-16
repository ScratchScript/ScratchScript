using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override IrNode? VisitVariableDeclarationStatement(
        ScratchScriptParser.VariableDeclarationStatementContext context)
    {
        var name = context.Identifier().GetText();

        // check if the name is available
        if (RequireIdentifierUnclaimedOrFail(name, context, context.Identifier()))
            return null;

        // in case of an ICE
        if (Visit(context.expression()) is not IrExpressionNode expression)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        if (_scope == null) throw new Exception("Cannot declare variables without a scope.");
        var node = new IrSetCommandNode(name, expression);

        _scope.Variables.Add(new ScratchScriptVariable(name, DetermineExpressionType(expression)));
        if (!LocationInformation.Variables.ContainsKey(_scope.Depth)) // since it's a nested dictionary
            LocationInformation.Variables[_scope.Depth] = new Dictionary<string, VariableLocationInformation>();
        LocationInformation.Variables[_scope.Depth][name] = new VariableLocationInformation
        {
            Context = context,
            Identifier = context.Identifier(),
            TypeSetterExpression = context.expression()
        };
        return node;
    }

    public override IrNode? VisitAssignmentStatement(ScratchScriptParser.AssignmentStatementContext context)
    {
        var name = context.Identifier().GetText();

        if (VisitIdentifier(name) is not IrExpressionNode variableIdentifier)
        {
            // TODO: expected identifier?
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.Identifier());
            return null;
        }

        // in case of an ICE
        if (Visit(context.expression()) is not IrExpressionNode expression)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        var assignmentOperator = context.assignmentOperators().GetText();
        if (assignmentOperator == null)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.assignmentOperators());
            return null;
        }

        if (_scope is FunctionScope function && function.Arguments.FirstOrDefault(arg => arg.Name == name) is
                { } argument)
        {
            if (argument.Type != DetermineExpressionType(expression))
            {
                var typeSetter = LocationInformation.Functions[function.FunctionName].ArgumentInformation[name]
                    .TypeSetter;
                if (typeSetter == null)
                    throw new Exception(
                        $"DiagnosticLocationStorage didn't contain the type setter for function argument \"{name}\" (function \"{function.FunctionName}\")");

                DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                    argument.Type, DetermineExpressionType(expression));
                DiagnosticReporter.Note((int)ScratchScriptNote.FunctionArgumentTypeSetAt, typeSetter, typeSetter);
                return null;
            }

            throw new Exception("Function argument assignment not supported yet");
            /*expression = ConvertAssignmentToBinaryExpression(assignmentOperator.Value,
                (ExpressionValue)Target.Function.GetArgument(_scope, name), expression);
            return Target.Function.HandleFunctionArgumentAssignment(_scope, name, expression);*/
        }

        if (_scope?.GetVariable(name) is not { } variable)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.VariableNotDefined, context, context.Identifier(), name);
            return null;
        }

        //NOTE: MustMatchTypesOrFail cannot be used here
        if (variable.Type != DetermineExpressionType(expression))
        {
            var locationInformation = LocationInformation.Variables[_scope.Depth][name];

            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(), variable.Type,
                DetermineExpressionType(expression));
            DiagnosticReporter.Note((int)ScratchScriptNote.VariableTypeSetAt, locationInformation.Context,
                locationInformation.TypeSetterExpression);
            return null;
        }

        return new IrSetCommandNode(variable.Name,
            ConvertAssignmentToBinaryExpression(assignmentOperator, variableIdentifier, expression));
    }

    // TODO: add bitwise operations support when imports are implemented
    private IrExpressionNode ConvertAssignmentToBinaryExpression(string op, IrExpressionNode variable,
        IrExpressionNode value)
    {
        if (_scope is null) throw new Exception("Cannot perform variable assignment in the root scope.");

        return op switch
        {
            "=" => value,
            "+=" => new IrBinaryExpressionNode(
                DetermineExpressionType(variable).Kind is ScratchTypeKind.String &&
                DetermineExpressionType(value).Kind is ScratchTypeKind.String
                    ? IrBinaryOperator.Join
                    : IrBinaryOperator.Add, variable, value),
            "-=" => new IrBinaryExpressionNode(IrBinaryOperator.Subtract, variable, value),
            "*=" => new IrBinaryExpressionNode(IrBinaryOperator.Multiply, variable, value),
            "/=" => new IrBinaryExpressionNode(IrBinaryOperator.Divide, variable, value),
            "%=" => new IrBinaryExpressionNode(IrBinaryOperator.Modulo, variable, value),
            "**=" => new IrBinaryExpressionNode(IrBinaryOperator.Power, variable, value),
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}
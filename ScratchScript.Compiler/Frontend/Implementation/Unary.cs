using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private IUnaryHandler _unaryHandler = null!;

    public override TypedValue? VisitUnaryAddExpression(ScratchScriptParser.UnaryAddExpressionContext context)
    {
        // get the operator
        if (Visit(context.addOperators()) is not GenericValue<AddOperator> op)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.addOperators());
            return null;
        }

        // get the left operand
        if (Visit(context.expression()) is not ExpressionValue operand)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        // operand must be a number
        if (operand.Type != ScratchType.Number)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Number, operand.Type);
            return null;
        }

        // evaluate at compile-time if possible
        if (Settings.UseConstantEvaluation && ConstantEvaluatorHelper.IsConstant(operand))
        {
            var value = ConstantEvaluatorHelper.Evaluate("* operand multiplier", new Dictionary<string, TypedValue>
            {
                ["operand"] = operand,
                ["multiplier"] = new TypedValue(op.Value == AddOperator.Plus ? 1 : -1, ScratchType.Number)
            });
            return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
        }

        return _unaryHandler.GetUnaryExpression(op.Value, operand);
    }

    public override TypedValue? VisitNotExpression(ScratchScriptParser.NotExpressionContext context)
    {
        // get the operand
        if (Visit(context.expression()) is not ExpressionValue operand)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        // operand must be a boolean
        if (operand.Type != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Boolean, operand.Type);
            return null;
        }

        // evaluate at compile-time if possible
        if (Settings.UseConstantEvaluation && ConstantEvaluatorHelper.IsConstant(operand))
        {
            var value = ConstantEvaluatorHelper.Evaluate("! operand",
                new Dictionary<string, TypedValue> { ["operand"] = operand });
            return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
        }

        // the not operator (!) is universal for all targets
        return new ExpressionValue($"! {operand.Value}", ScratchType.Boolean, operand.Dependencies, operand.Cleanup);
    }
}
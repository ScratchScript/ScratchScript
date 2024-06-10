using System.Diagnostics;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitBinaryBitwiseExpression(ScratchScriptParser.BinaryBitwiseExpressionContext context)
    {
        // get the operator
        if (Visit(context.bitwiseOperators()) is not GenericValue<BitwiseOperators> op)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.bitwiseOperators());
            return null;
        }

        // get the left operand
        if (Visit(context.expression(0)) is not ExpressionValue left)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(1));
            return null;
        }

        // left and right operands must be a number
        if (MustMatchTypeOrFail(left, ScratchType.Number, context, context.expression(0))) return null;
        if (MustMatchTypeOrFail(right, ScratchType.Number, context, context.expression(1))) return null;

        // evaluate at compile-time if possible
        if (Settings.UseConstantEvaluation && ConstantEvaluatorHelper.IsConstant(left) &&
            ConstantEvaluatorHelper.IsConstant(right))
        {
            var value = ConstantEvaluatorHelper.Evaluate($"{context.bitwiseOperators().GetText()} left right",
                new Dictionary<string, TypedValue> { ["left"] = left, ["right"] = right });
            return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
        }

        Debug.Assert(_scope != null, nameof(_scope) + " != null");
        return Target.Binary.GetBinaryBitwiseExpression(_scope, op.Value, left, right);
    }

    public override TypedValue? VisitBinaryCompareExpression(ScratchScriptParser.BinaryCompareExpressionContext context)
    {
        // get the operator
        if (Visit(context.compareOperators()) is not GenericValue<CompareOperators> op)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.compareOperators());
            return null;
        }

        // get the left operand
        if (Visit(context.expression(0)) is not ExpressionValue left)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(1));
            return null;
        }

        // types of operands must match
        if (MustMatchTypeOrFail(left, right.Type, context, context.expression(0))) return null;
        if (MustMatchTypeOrFail(right, left.Type, context, context.expression(1))) return null;

        // evaluate at compile-time if possible
        if (Settings.UseConstantEvaluation && ConstantEvaluatorHelper.IsConstant(left) &&
            ConstantEvaluatorHelper.IsConstant(right))
        {
            var value = ConstantEvaluatorHelper.Evaluate($"{context.compareOperators().GetText()} left right",
                new Dictionary<string, TypedValue> { ["left"] = left, ["right"] = right });
            return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
        }

        Debug.Assert(_scope != null, nameof(_scope) + " != null");

        if (op.Value is not (CompareOperators.Equal or CompareOperators.NotEqual))
            return Target.Binary.GetBinaryNumberComparisonExpression(_scope, op.Value, left, right);

        //TODO: this logic should be updated to handle enums and custom types in the future

        var equalExpression = left.Type == ScratchType.Number
            ? Target.Binary.GetBinaryNumberEquationExpression(_scope, left, right)
            : Target.Binary.GetBinaryStringEquationExpression(_scope, left, right);

        if (op.Value == CompareOperators.Equal) return equalExpression;

        return new ExpressionValue($"! {equalExpression.Value}", equalExpression.Type, equalExpression.Dependencies,
            equalExpression.Cleanup);
    }

    public override TypedValue? VisitBinaryMultiplyExpression(
        ScratchScriptParser.BinaryMultiplyExpressionContext context)
    {
        // get the operator
        if (Visit(context.multiplyOperators()) is not GenericValue<MultiplyOperators> op)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.multiplyOperators());
            return null;
        }

        // get the left operand
        if (Visit(context.expression(0)) is not ExpressionValue left)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(1));
            return null;
        }

        // left and right operands must be a number
        if (MustMatchTypeOrFail(left, ScratchType.Number, context, context.expression(0))) return null;
        if (MustMatchTypeOrFail(right, ScratchType.Number, context, context.expression(1))) return null;

        // division by zero check
        if (right.Value is (double)0 && op.Value == MultiplyOperators.Divide)
            DiagnosticReporter.Warning((int)ScratchScriptWarning.DivisionByZero, context, context);

        // evaluate at compile-time if possible
        if (Settings.UseConstantEvaluation && ConstantEvaluatorHelper.IsConstant(left) &&
            ConstantEvaluatorHelper.IsConstant(right))
        {
            var value = ConstantEvaluatorHelper.Evaluate($"{context.multiplyOperators().GetText()} left right",
                new Dictionary<string, TypedValue> { ["left"] = left, ["right"] = right });
            return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
        }

        Debug.Assert(_scope != null, nameof(_scope) + " != null");
        return Target.Binary.GetBinaryMultiplyExpression(_scope, op.Value, left, right);
    }

    public override TypedValue? VisitBinaryBooleanExpression(ScratchScriptParser.BinaryBooleanExpressionContext context)
    {
        // get the operator
        if (Visit(context.booleanOperators()) is not GenericValue<BooleanOperator>)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.booleanOperators());
            return null;
        }

        // get the left operand
        if (Visit(context.expression(0)) is not ExpressionValue left)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(1));
            return null;
        }

        // left and right operands must be a boolean
        if (MustMatchTypeOrFail(left, ScratchType.Boolean, context, context.expression(0))) return null;
        if (MustMatchTypeOrFail(right, ScratchType.Boolean, context, context.expression(1))) return null;

        // evaluate at compile-time if possible
        if (Settings.UseConstantEvaluation && ConstantEvaluatorHelper.IsConstant(left) &&
            ConstantEvaluatorHelper.IsConstant(right))
        {
            var value = ConstantEvaluatorHelper.Evaluate($"{context.booleanOperators().GetText()} left right",
                new Dictionary<string, TypedValue> { ["left"] = left, ["right"] = right });
            return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
        }

        // the operator in the IR and ScratchScript *should* match
        var irOperator = context.booleanOperators().GetText()!;
        return new ExpressionValue($"{irOperator} {left.Value} {right.Value}", ScratchType.Boolean,
            left.Dependencies.ConcatNullable(right.Dependencies),
            left.Cleanup.ConcatNullable(right.Dependencies));
    }

    public override TypedValue? VisitBinaryAddExpression(ScratchScriptParser.BinaryAddExpressionContext context)
    {
        // get the operator
        if (Visit(context.addOperators()) is not GenericValue<AddOperator> op)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.addOperators());
            return null;
        }

        // get the left operand
        if (Visit(context.expression(0)) is not ExpressionValue left)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression(1));
            return null;
        }

        // the operator in the IR and ScratchScript *should* match (except for the string join operator, "~")
        var irOperator = context.addOperators().GetText()!;
        var resultType = ScratchType.Number;

        // this is a string join expression
        if (op.Value == AddOperator.Plus && (left.Type == ScratchType.String || right.Type == ScratchType.String))
        {
            if (MustMatchTypeOrFail(left, ScratchType.String, context, context.expression(0))) return null;
            if (MustMatchTypeOrFail(right, ScratchType.String, context, context.expression(1))) return null;

            irOperator = "~";
            resultType = ScratchType.String;
        }

        // this is a regular +/- expression
        else
        {
            if (MustMatchTypeOrFail(left, ScratchType.Number, context, context.expression(0))) return null;
            if (MustMatchTypeOrFail(right, ScratchType.Number, context, context.expression(1))) return null;
        }

        // evaluate at compile-time if possible
        if (Settings.UseConstantEvaluation && ConstantEvaluatorHelper.IsConstant(left) &&
            ConstantEvaluatorHelper.IsConstant(right))
        {
            var value = ConstantEvaluatorHelper.Evaluate($"{irOperator} left right",
                new Dictionary<string, TypedValue> { ["left"] = left, ["right"] = right });
            return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
        }

        return new ExpressionValue($"{irOperator} {left.Value} {right.Value}", resultType,
            left.Dependencies.ConcatNullable(right.Dependencies),
            left.Cleanup.ConcatNullable(right.Cleanup));
    }
}
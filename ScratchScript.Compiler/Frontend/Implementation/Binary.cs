﻿using System.Diagnostics;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitBinaryBitwiseExpression(ScratchScriptParser.BinaryBitwiseExpressionContext context)
    {
        return base.VisitBinaryBitwiseExpression(context);
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
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(1));
            return null;
        }

        // left operand must be a number
        if (left.Type != ScratchType.Number)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(0),
                ScratchType.Number, left.Type);
            return null;
        }

        // right operand must also be a number
        if (right.Type != ScratchType.Number)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(1),
                ScratchType.Number, right.Type);
            return null;
        }

        Debug.Assert(_scope != null, nameof(_scope) + " != null");
        return _binaryHandler.GetBinaryMultiplyExpression(ref _scope, op.Value, left, right);
    }

    public override TypedValue? VisitBinaryBitwiseShiftExpression(
        ScratchScriptParser.BinaryBitwiseShiftExpressionContext context)
    {
        return base.VisitBinaryBitwiseShiftExpression(context);
    }

    public override TypedValue? VisitBinaryCompareExpression(ScratchScriptParser.BinaryCompareExpressionContext context)
    {
        return base.VisitBinaryCompareExpression(context);
    }

    public override TypedValue? VisitBinaryBooleanExpression(ScratchScriptParser.BinaryBooleanExpressionContext context)
    {
        // get the operator
        if (Visit(context.booleanOperators()) is not GenericValue<BooleanOperator> op)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.booleanOperators());
            return null;
        }

        // get the left operand
        if (Visit(context.expression(0)) is not ExpressionValue left)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(1));
            return null;
        }

        // left operand must be a boolean
        if (left.Type != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(0),
                ScratchType.Boolean, left.Type);
            return null;
        }

        // right operand must also be a boolean
        if (right.Type != ScratchType.Boolean)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(1),
                ScratchType.Boolean, right.Type);
            return null;
        }

        // the operator in the IR and ScratchScript *should* match (except for the string join operator, "~")
        var irOperator = context.booleanOperators().GetText()!;
        return new ExpressionValue($"{irOperator} {left.Value} {right.Value}", ScratchType.Boolean,
            left.Dependencies.Combine(Settings.CommandSeparator, right.Dependencies),
            left.Cleanup.Combine(Settings.CommandSeparator, right.Cleanup));
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
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(0));
            return null;
        }

        // get the right operand
        if (Visit(context.expression(1)) is not ExpressionValue right)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(1));
            return null;
        }

        // the operator in the IR and ScratchScript *should* match (except for the string join operator, "~")
        var irOperator = context.addOperators().GetText()!;
        var resultType = ScratchType.Number;

        // this is a string join expression
        if (op.Value == AddOperator.Plus && left.Type == ScratchType.String)
        {
            if (right.Type != ScratchType.String)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(1),
                    ScratchType.String, right.Type);
                return null;
            }

            irOperator = "~";
            resultType = ScratchType.String;
        }

        // this is a regular +/- expression
        else
        {
            if (left.Type != ScratchType.Number)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(0),
                    ScratchType.Number, left.Type);
                return null;
            }

            if (right.Type != ScratchType.Number)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(1),
                    ScratchType.Number, right.Type);
                return null;
            }
        }

        return new ExpressionValue($"{irOperator} {left.Value} {right.Value}", resultType,
            left.Dependencies.Combine(Settings.CommandSeparator, right.Dependencies),
            left.Cleanup.Combine(Settings.CommandSeparator, right.Cleanup));
    }
}
﻿using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
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
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        // operand must be a number
        if (operand.Type != ScratchType.Number)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                ScratchType.Number, operand.Type);
            return null;
        }

        return _unaryHandler.GetUnaryExpression(op.Value, operand);
    }
}
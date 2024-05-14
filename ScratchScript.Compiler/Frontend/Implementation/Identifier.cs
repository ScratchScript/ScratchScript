﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private (ParserRuleContext Context, ITerminalNode Identifier)? CheckIdentifierUsage(string identifier)
    {
        if (Exports.Enums.ContainsKey(identifier))
            return (LocationInformation.Enums[identifier].Context, LocationInformation.Enums[identifier].Identifier);
        if (Exports.Events.ContainsKey(identifier))
            return (LocationInformation.Events[identifier].Context, LocationInformation.Events[identifier].Identifier);
        if (_scope?.GetVariableDepth(identifier) is {} variableDepth)
            return (LocationInformation.Variables[variableDepth][identifier].Context,
                LocationInformation.Variables[variableDepth][identifier].Identifier);

        return null;
    }

    private bool RequireIdentifierUnclaimedOrFail(string identifier, ParserRuleContext ownContext,
        ITerminalNode ownIdentifier)
    {
        var usage = CheckIdentifierUsage(identifier);
        if (!usage.HasValue) return false;

        DiagnosticReporter.Error((int)ScratchScriptError.IdentifierAlreadyClaimed, ownContext, ownIdentifier,
            identifier);
        DiagnosticReporter.Note((int)ScratchScriptNote.IdentifierClaimedAt, usage.Value.Context,
            usage.Value.Identifier);
        return true;
    }

    public override TypedValue? VisitIdentifierExpression(ScratchScriptParser.IdentifierExpressionContext context)
    {
        if (VisitIdentifier(context.Identifier().GetText()) is not { } result)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.UnknownIdentifier, context, context.Identifier(), context.Identifier().GetText());
            return null;
        }

        return new ExpressionValue(result.Value, result.Type);
    }

    private TypedValue? VisitIdentifier(string identifier)
    {
        if (_scope?.GetVariable(identifier) is { } variable)
            return new TypedValue(BackendHelper.GetVariableValue(variable.Id), variable.Type);
        
        return null;
    }
}
using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Targets;
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
        if (_scope?.GetVariableDepth(identifier) is { } variableDepth)
            return (LocationInformation.Variables[variableDepth][identifier].Context,
                LocationInformation.Variables[variableDepth][identifier].Identifier);

        if (_scope is IFunctionScope functionScope)
        {
            if (functionScope.Arguments.Any(arg => arg.Name == identifier))
                return (LocationInformation.Functions[functionScope.FunctionName].DefinitionContext,
                    LocationInformation.Functions[functionScope.FunctionName].ArgumentInformation[identifier]
                        .Identifier);
            if (functionScope.FunctionName == identifier)
                return (LocationInformation.Functions[functionScope.FunctionName].DefinitionContext,
                    LocationInformation.Functions[functionScope.FunctionName].FunctionNameIdentifier);
        }

        return null;
    }

    private bool MustMatchTypeOrFail(TypedValue value, ScratchType expected, ParserRuleContext ownContext,
        ParserRuleContext ownSource)
    {
        if (expected == ScratchType.Unknown) return false;

        //TODO: handle function arguments
        if (value.Type != expected)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, ownContext, ownSource, expected, value.Type);
            return true;
        }

        return false;
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
        var name = context.Identifier().GetText();

        if (VisitIdentifier(name) is not { } result)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.UnknownIdentifier, context, context.Identifier(),
                name);
            return null;
        }

        var type = result is TypeDeclarationValue
            ? IdentifierType.CustomType
            : _scope is IFunctionScope
                ? IdentifierType.FunctionArgument
                : IdentifierType.Variable;

        return new IdentifierExpressionValue(
            type, name, _scope, result.Value,
            result.Type);
    }

    private TypedValue? VisitIdentifier(string identifier)
    {
        Debug.Assert(_scope != null, nameof(_scope) + " != null");

        if (_scope.GetVariable(identifier) is { } variable)
            return _dataHandler.GetVariable(_scope, variable);
        if (_scope is IFunctionScope functionScope && functionScope.Arguments.Any(arg => arg.Name == identifier))
            return _functionHandler.GetArgument(_scope, identifier);
        if (Exports.Enums.TryGetValue(identifier, out var type))
            return new TypeDeclarationValue(type);

        return null;
    }
}
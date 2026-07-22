using System.Diagnostics;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor
{
    private (ParserRuleContext Context, ITerminalNode Identifier)? CheckIdentifierUsage(string identifier)
    {
        /*if (Exports.Enums.ContainsKey(identifier))
            return (LocationInformation.Enums[identifier].Context, LocationInformation.Enums[identifier].Identifier);*/
        if (Exports.Events.ContainsKey(identifier))
            return (LocationInformation.Events[identifier].Context, LocationInformation.Events[identifier].Identifier);
        if (_scope?.GetVariableDepth(identifier) is { } variableDepth)
            return (LocationInformation.Variables[variableDepth][identifier].Context,
                LocationInformation.Variables[variableDepth][identifier].Identifier);

        if (_scope is FunctionScope functionScope)
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

    private bool RequireIdentifierUnclaimedOrFail(string identifier, ParserRuleContext ownContext,
        ITerminalNode ownIdentifier)
    {
        var usage = CheckIdentifierUsage(identifier);
        if (!usage.HasValue) return false;

        DiagnosticReporter.Instance.Error((int)ScratchScriptError.IdentifierAlreadyClaimed, ownContext, ownIdentifier,
            identifier);
        DiagnosticReporter.Instance.Note((int)ScratchScriptNote.IdentifierClaimedAt, usage.Value.Context,
            usage.Value.Identifier);
        return true;
    }

    public override IrNode? VisitIdentifierExpression(ScratchScriptParser.IdentifierExpressionContext context)
    {
        var identifier = context.Identifier().GetText();
        return VisitIdentifier(identifier).WithContext(context);
    }

    private IrNode? VisitIdentifier(string identifier)
    {
        Debug.Assert(_scope != null, nameof(_scope) + " != null");
        if (_scope.GetVariable(identifier) is { } variable)
            return new IrLocalVariableIdentifierExpressionNode(variable.Name);
        if (_scope.GetArgument(identifier) is { } argument)
            return new IrFunctionArgumentExpressionNode(argument.Name);
        /*if (Exports.Enums.TryGetValue(identifier, out var type))
            return new TypeDeclarationValue(type);*/

        return null;
    }
}
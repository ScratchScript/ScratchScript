using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitMemberPropertyAccessExpression(
        ScratchScriptParser.MemberPropertyAccessExpressionContext context)
    {
        var property = context.Identifier().GetText();

        if (Visit(context.expression()) is not { } value)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        if (value is IdentifierExpressionValue identifierExpressionValue)
        {
            if (_scope == null) throw new Exception("Cannot perform member access expressions in the root scope.");

            var identifierValue = identifierExpressionValue.IdentifierType switch
            {
                IdentifierType.CustomType => null,
                IdentifierType.FunctionArgument => _functionHandler.GetArgument(_scope,
                    identifierExpressionValue.Identifier),
                IdentifierType.Variable => _dataHandler.GetVariable(_scope,
                    _scope.GetVariable(identifierExpressionValue.Identifier) ?? throw new Exception(
                        $"No variable with the name \"{identifierExpressionValue.Identifier}\" exists, despite being returned from VisitIdentifier.")),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (identifierExpressionValue.IdentifierType == IdentifierType.CustomType)
                if (Exports.Enums.TryGetValue(identifierExpressionValue.Identifier, out var enumType))
                {
                    if (enumType.Values.ContainsKey(property))
                        return new ExpressionValue($"\"{identifierExpressionValue.Identifier}_{property}\"", enumType);

                    DiagnosticReporter.Error((int)ScratchScriptError.EnumEntryNotFound, context, context.Identifier(),
                        property, identifierExpressionValue.Identifier);
                    DiagnosticReporter.Note((int)ScratchScriptNote.ListEnumEntries,
                        string.Join(", ", enumType.Values.Keys));
                    return null;
                }

            if (value.Type is EnumScratchType valueEnumType && property is "value" or "name")
            {
                if (property == "value")
                    return _enumHandler.GetEnumValue(valueEnumType, identifierValue!);
                if (property == "name")
                    return _enumHandler.GetEnumName(valueEnumType, identifierValue!);
            }
            else
            {
                DiagnosticReporter.Error((int)ScratchScriptError.NoPropertyDefined, context, context.Identifier(),
                    property,
                    value.Type.ToString());
                DiagnosticReporter.Note((int)ScratchScriptNote.ListEnumProperties,
                    string.Join(", ", ["value", "name"]));
                return null;
            }
        }

        return null;
    }
}
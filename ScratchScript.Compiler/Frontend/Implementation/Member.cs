using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
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

        if (value is IdentifierExpressionValue { IdentifierType: IdentifierType.CustomType } identifierExpressionValue)
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
            var reference = GetValueReference(value);
            switch (property)
            {
                case "value":
                    return _enumHandler.GetEnumValue(valueEnumType, reference);
                case "name":
                    return _enumHandler.GetEnumName(valueEnumType, reference);
            }
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

        return null;
    }

    private TypedValue GetValueReference(TypedValue original)
    {
        if (_scope == null) throw new Exception("Cannot get a reference to a value in the root scope.");

        return original switch
        {
            IdentifierExpressionValue identifierExpression => identifierExpression.IdentifierType switch
            {
                IdentifierType.FunctionArgument => _functionHandler.GetArgument(_scope,
                    identifierExpression.Identifier),
                IdentifierType.Variable => _dataHandler.GetVariable(_scope,
                    _scope.GetVariable(identifierExpression.Identifier) ?? throw new Exception(
                        $"No variable with the name \"{identifierExpression.Identifier}\" exists, despite being returned from VisitIdentifier.")),
                _ => identifierExpression
            },
            _ => original
        };
    }
}
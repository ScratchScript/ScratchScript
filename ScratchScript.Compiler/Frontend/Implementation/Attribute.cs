using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private IAttributeHandler _attributeHandler = null!;

    private (string Name, IEnumerable<TypedValue> Values)? ProcessAttribute(
        ScratchScriptParser.AttributeStatementContext context, bool topLevel)
    {
        var name = context.Identifier().GetText();

        var constants = context.constant().Select(Visit).ToList();
        for (var idx = 0; idx < constants.Count; idx++)
        {
            if (constants[idx] != null) continue;

            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.constant(idx));
            return null;
        }

        var filteredConstants = constants.OfType<TypedValue>().ToList();

        var signature = filteredConstants.Select(constant => constant.Type).ToList();
        var signatureString = StringExtensions.GetFunctionSignatureString(name, signature);
        if ((topLevel && !_attributeHandler.TopLevelAttributes.Contains(signatureString)) ||
            (!topLevel && !_attributeHandler.FunctionAttributes.Contains(signatureString)))
        {
            DiagnosticReporter.Error((int)ScratchScriptError.NoAttributeWithMatchingSignatureFound, context, context,
                StringExtensions.GetFunctionSignatureString(name, signature));
            return null;
        }

        return (name, filteredConstants);
    }

    public override TypedValue? VisitAttributeStatement(ScratchScriptParser.AttributeStatementContext context)
    {
        var result = ProcessAttribute(context, true);
        if (result == null) return null;
        _attributeHandler.ProcessTopLevelAttribute(result.Value.Name, result.Value.Values);
        return null;
    }
}
using System.Diagnostics;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitEnumDeclarationStatement(
        ScratchScriptParser.EnumDeclarationStatementContext context)
    {
        var name = context.Identifier().GetText();
        var values = new Dictionary<string, EnumEntryValue>();

        // check if the name can be used
        if (RequireIdentifierUnclaimedOrFail(name, context, context.Identifier()))
            return null;

        var locationInformation = new EnumLocationInformation
        {
            EntryDeclarations = [],
            Context = context,
            Identifier = context.Identifier()
        };

        var valueType = ScratchType.Unknown;
        foreach (var entryContext in context.enumEntry())
        {
            // in case of an ICE
            if (VisitEnumEntry(entryContext) is not EnumEntryValue entry)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, entryContext);
                return null;
            }

            // check for duplicate identifiers (entries)
            if (values.ContainsKey(entry.Name))
            {
                var (statement, _) = locationInformation.EntryDeclarations[entry.Name];
                DiagnosticReporter.Error((int)ScratchScriptError.EnumEntryAlreadyDeclared, entryContext,
                    entryContext.Identifier(), entry.Name);
                DiagnosticReporter.Note((int)ScratchScriptNote.EnumEntryDeclaredAt, statement, statement);
                return null;
            }

            // if the enum type is unknown and a constant is found, make the whole enum use the same type
            if (entry.Type != ScratchType.Unknown && valueType == ScratchType.Unknown)
            {
                valueType = entry.Type;
                locationInformation.TypeSetterStatement = entryContext;
                locationInformation.TypeSetterAssignment = entryContext.constant();
            }

            // if the constant's type doesn't match the enum type
            if (entry.Value != null && entry.Type != valueType)
            {
                Debug.Assert(locationInformation.TypeSetterStatement != null,
                    "locationInformation.TypeSetterStart != null");
                Debug.Assert(locationInformation.TypeSetterAssignment != null,
                    "locationInformation.TypeSetterAssignment != null");

                DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, entryContext, entryContext.constant(),
                    valueType, entry.Type);
                DiagnosticReporter.Note((int)ScratchScriptNote.EnumTypeSetAt, locationInformation.TypeSetterStatement,
                    locationInformation.TypeSetterAssignment);

                return null;
            }

            // if the constant isn't present but the enum is non-numeric
            if (entry.Value == null && valueType.Kind is not ScratchTypeKind.Unknown and ScratchTypeKind.Number)
            {
                Debug.Assert(locationInformation.TypeSetterStatement != null,
                    "locationInformation.TypeSetterStart != null");
                Debug.Assert(locationInformation.TypeSetterAssignment != null,
                    "locationInformation.TypeSetterAssignment != null");

                DiagnosticReporter.Error((int)ScratchScriptError.NonNumericEntryMustSpecifyAllValues, entryContext,
                    entryContext);
                DiagnosticReporter.Note((int)ScratchScriptNote.EnumTypeSetAt, locationInformation.TypeSetterStatement,
                    locationInformation.TypeSetterAssignment);

                return null;
            }

            locationInformation.EntryDeclarations[entry.Name] = (entryContext, entryContext.constant());
            values[entry.Name] = entry;
        }

        // assign default values to entries with no initializer
        if (valueType.Kind is ScratchTypeKind.Number or ScratchTypeKind.Unknown)
        {
            valueType = ScratchType.Number;
            var currentValue = (double)0;

            foreach (var (key, entry) in values)
                if (entry.Value == null)
                {
                    values[key] = entry with { Value = currentValue, Type = ScratchType.Number };
                    currentValue++;
                }
                else
                {
                    currentValue = (double)entry.Value;
                }
        }

        // check that all values have a value (i.e. if the enum type was assigned by the last element)
        else
        {
            foreach (var (key, entry) in values)
            {
                if (entry.Value != null) continue;

                Debug.Assert(locationInformation.TypeSetterStatement != null,
                    "locationInformation.TypeSetterStart != null");
                Debug.Assert(locationInformation.TypeSetterAssignment != null,
                    "locationInformation.TypeSetterAssignment != null");

                var (statement, assignment) = locationInformation.EntryDeclarations[key];

                DiagnosticReporter.Error((int)ScratchScriptError.NonNumericEntryMustSpecifyAllValues,
                    statement,
                    assignment ?? statement,
                    valueType, entry.Type);
                DiagnosticReporter.Note((int)ScratchScriptNote.EnumTypeSetAt,
                    locationInformation.TypeSetterStatement,
                    locationInformation.TypeSetterAssignment);

                return null;
            }
        }

        LocationInformation.Enums[name] = locationInformation;
        Exports.Enums[name] = new EnumScratchType(name, valueType, values);
        return null;
    }

    public override TypedValue? VisitEnumEntry(ScratchScriptParser.EnumEntryContext context)
    {
        if (context.constant() == null)
            return new EnumEntryValue(context.Identifier().GetText(), null, ScratchType.Unknown);

        var value = VisitConstant(context.constant());
        if (value == null)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context.constant(), context.constant());
            return null;
        }

        return new EnumEntryValue(context.Identifier().GetText(), value.Value, value.Type);
    }
}
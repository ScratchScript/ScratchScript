using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor
{
    public override IrNode? VisitEnumDeclarationStatement(
        ScratchScriptParser.EnumDeclarationStatementContext context)
    {
        var name = context.Identifier().GetText();
        var entries = new SortedDictionary<string, IrExpressionNode?>();

        // check if the name can be used
        if (RequireIdentifierUnclaimedOrFail(name, context, context.Identifier()))
            return null;

        var locationInformation = new EnumLocationInformation
        {
            EntryDeclarations = [],
            Context = context,
            Identifier = context.Identifier()
        };

        foreach (var entryContext in context.enumEntry())
        {
            var entryName = entryContext.Identifier().GetText();
            IrExpressionNode? entryValue = null;

            if (entryContext.expression() != null)
            {
                if (Visit(entryContext.expression()) is not IrExpressionNode expression)
                {
                    // todo: expected expression
                    DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, entryContext);
                    return null;
                }

                entryValue = expression;
            }

            // check for duplicate identifiers (entries)
            if (entries.ContainsKey(entryName))
            {
                var (statement, _) = locationInformation.EntryDeclarations[entryName];
                DiagnosticReporter.Instance.Error((int)ScratchScriptError.EnumEntryAlreadyDeclared, entryContext,
                    entryContext.Identifier(), entryName);
                DiagnosticReporter.Instance.Note((int)ScratchScriptNote.EnumEntryDeclaredAt, statement, statement);
                return null;
            }

            locationInformation.EntryDeclarations[entryName] = (entryContext, entryContext.expression());
            entries[entryName] = entryValue?.WithContext(entryContext.expression());
        }

        LocationInformation.Enums[name] = locationInformation;
        Symbols[Namespace][Artifact].Enums.Add(name);
        return new IrEnumNode(name, entries);
    }
}
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Information;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override IrNode? VisitEventStatement(ScratchScriptParser.EventStatementContext context)
    {
        var eventName = context.Identifier().GetText();

        // check for duplicate event declarations
        // NOTE: there can't be multiple events of the same type declared because that would cause race conditions
        if (Exports.Events.ContainsKey(eventName))
        {
            DiagnosticReporter.Error((int)ScratchScriptError.EventAlreadyDeclared, context, context.Identifier(),
                eventName);
            DiagnosticReporter.Note((int)ScratchScriptNote.EventDeclaredAt,
                LocationInformation.Events[eventName].Context, LocationInformation.Events[eventName].Identifier);
            return null;
        }

        // check if the name can be used
        if (RequireIdentifierUnclaimedOrFail(eventName, context, context.Identifier()))
            return null;

        var locationInformation = new EventLocationInformation
        {
            Context = context,
            Identifier = context.Identifier()
        };

        // in case of an ICE
        if (VisitBlock(context.block()) is not IrBlockNode blockNode)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context.block(), context.block());
            return null;
        }

        LocationInformation.Events[eventName] = locationInformation;
        var node = new IrEventNode(eventName, blockNode.Scope);
        Exports.Events[eventName] = node;
        return node;
    }
}
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ScratchScript.Compiler.Diagnostics;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public (ParserRuleContext Context, ITerminalNode Identifier)? CheckIdentifierUsage(string identifier)
    {
        if (Exports.Enums.ContainsKey(identifier))
            return (LocationInformation.Enums[identifier].Context, LocationInformation.Enums[identifier].Identifier);
        if (Exports.Events.ContainsKey(identifier))
            return (LocationInformation.Events[identifier].Context, LocationInformation.Events[identifier].Identifier);
        
        return null;
    }

    public bool RequireIdentifierUnclaimedOrFail(string identifier, ParserRuleContext ownContext, ITerminalNode ownIdentifier)
    {
        var usage = CheckIdentifierUsage(identifier);
        if (!usage.HasValue) return false;
        
        DiagnosticReporter.Error((int)ScratchScriptError.IdentifierAlreadyClaimed, ownContext, ownIdentifier,
            identifier);
        DiagnosticReporter.Note((int)ScratchScriptNote.IdentifierClaimedAt, usage.Value.Context,
            usage.Value.Identifier);
        return true;
    }
}
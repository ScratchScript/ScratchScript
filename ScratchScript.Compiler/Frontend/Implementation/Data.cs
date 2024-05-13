using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitVariableDeclarationStatement(ScratchScriptParser.VariableDeclarationStatementContext context)
    {
        var name = context.Identifier().GetText();
        
        // check if the name is available
        if (RequireIdentifierUnclaimedOrFail(name, ownContext: context, ownIdentifier: context.Identifier())) return null;

        // in case of an ICE
        if (Visit(context.expression()) is not ExpressionValue expression)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }
        
        Scope!.AddVariable(name, expression);
        return null;
    }
}
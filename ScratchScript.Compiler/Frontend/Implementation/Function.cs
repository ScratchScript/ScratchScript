using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitMemberFunctionCallStatement(
        ScratchScriptParser.MemberFunctionCallStatementContext context)
    {
        return base.VisitMemberFunctionCallStatement(context);
    }

    public override TypedValue? VisitFunctionCallStatement(ScratchScriptParser.FunctionCallStatementContext context)
    {
        return base.VisitFunctionCallStatement(context);
    }

    public override TypedValue? VisitFunctionCallExpression(ScratchScriptParser.FunctionCallExpressionContext context)
    {
        return base.VisitFunctionCallExpression(context);
    }

    public override TypedValue? VisitMemberFunctionCallExpression(
        ScratchScriptParser.MemberFunctionCallExpressionContext context)
    {
        return base.VisitMemberFunctionCallExpression(context);
    }

    public override TypedValue? VisitFunctionDeclarationStatement(
        ScratchScriptParser.FunctionDeclarationStatementContext context)
    {
        var name = context.Identifier().GetText();

        // verify that the name can be used
        if (RequireIdentifierUnclaimedOrFail(name, context, context.Identifier())) return null;

        var scope = CreateFunctionScope();
        scope.FunctionName = name;

        // register the arguments before parsing the block
        foreach (var identifier in context.typedIdentifier())
        {
            var argumentName = identifier.Identifier().GetText();
            var argumentType = ScratchType.Unknown;

            // if the type is specified
            if (identifier.type() != null && Visit(identifier.type()) is TypeDeclarationValue typeDeclarationValue)
                argumentType = typeDeclarationValue.Type;

            if (RequireIdentifierUnclaimedOrFail(argumentName, context, identifier.Identifier())) return null;

            // all functions are top-level (for now) so the scope depth will always be 0
            scope.Arguments[argumentName] = new ScratchScriptVariable(argumentName,
                _dataHandler.GenerateVariableId(0, Id, argumentName), argumentType);
        }

        return null;
    }
}
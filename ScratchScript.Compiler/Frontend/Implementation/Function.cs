using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private IFunctionHandler _functionHandler = null!;
    
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
        // TODO: this technically should be implemented by doing function overloading
        // but for now, basic functions will do. implement it when functions are stable though!
        if (RequireIdentifierUnclaimedOrFail(name, context, context.Identifier())) return null;

        var scope = CreateFunctionScope();
        scope.FunctionName = name;
        scope.Header = $"block {name}";
        
        var locationInformation = new FunctionLocationInformation
        {
            DefinitionContext = context,
            FunctionNameIdentifier = context.Identifier(),
            ArgumentInformation = []
        };

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
            locationInformation.ArgumentInformation[argumentName] = (identifier.Identifier(), identifier.type());
        }
        
        // set the LocationInformation before visiting the scope as any identifier checks
        // will fail to point the location of the acquirer otherwise.
        LocationInformation.Functions[name] = locationInformation;
        
        scope = VisitBlock(scope, context.block()).Scope as FunctionScope;
        Exports.Functions[name] = scope ?? throw new Exception("The scope returned from VisitBlock() was null.");
        return null;
    }
}
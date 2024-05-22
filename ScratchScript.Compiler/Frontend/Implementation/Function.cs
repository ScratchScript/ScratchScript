using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private IFunctionHandler _functionHandler = null!;

    private IFunctionScope? FindFunction(string name, IEnumerable<ScratchType> signature)
    {
        return Exports.Functions.Values.FirstOrDefault(func =>
            func.FunctionName == name && func.Arguments.Select(arg => arg.Type).SequenceEqual(signature));
    }

    private (IFunctionScope? Function, ExpressionValue? ReturnValue) HandleFunctionCall(
        ScratchScriptParser.FunctionCallStatementContext context)
    {
        if (_scope is null) return (null, null);

        var name = context.Identifier().GetText();

        // strict name check (without signature)
        if (!Exports.Functions.ContainsKey(name))
        {
            DiagnosticReporter.Error((int)ScratchScriptError.NoFunctionsWithNameAreDefined, context,
                context.Identifier(), name);
            return (null, null);
        }

        var arguments = new List<ExpressionValue>();
        // todo: add support for named arguments later (involves combinations and permutations and stuff)
        foreach (var arg in context.functionArgument())
        {
            if (Visit(arg.expression()) is not ExpressionValue expression)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, arg.expression());
                return (null, null);
            }

            arguments.Add(expression);
        }

        var signature = arguments.Select(arg => arg.Type).ToList();
        var function = FindFunction(name, signature);

        if (function == null)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.NoFunctionWithMatchingSignatureDefined, context, context,
                StringExtensions.GetFunctionSignatureString(name, signature));
            return (null, null);
        }

        var value = _functionHandler.HandleFunctionCall(ref _scope, function, arguments);
        return (function, value);
    }

    public override TypedValue? VisitMemberFunctionCallStatement(
        ScratchScriptParser.MemberFunctionCallStatementContext context)
    {
        return base.VisitMemberFunctionCallStatement(context);
    }

    public override TypedValue? VisitFunctionCallStatement(ScratchScriptParser.FunctionCallStatementContext context)
    {
        var (function, returnValue) = HandleFunctionCall(context);
        if (function == null) return null;
        if (returnValue != null)
            DiagnosticReporter.Warning((int)ScratchScriptWarning.UnusedFunctionReturnValue, context, context,
                function.SignatureString, function.ReturnType.ToString());

        return null;
    }

    public override TypedValue? VisitFunctionCallExpression(ScratchScriptParser.FunctionCallExpressionContext context)
    {
        var (function, returnValue) = HandleFunctionCall(context.functionCallStatement());
        if (function == null) return null;
        if (returnValue == null)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.FunctionDoesNotReturnAValue, context, context,
                function.SignatureString);
            return null;
        }

        return returnValue;
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
        scope.ReturnType = ScratchType.Unknown;
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
            scope.Arguments.Add(new ScratchScriptVariable(argumentName,
                _dataHandler.GenerateVariableId(0, Id, argumentName), argumentType));
            locationInformation.ArgumentInformation[argumentName] = (identifier.Identifier(), identifier.type());
        }

        // set the LocationInformation before visiting the scope as any identifier checks
        // will fail to point the location of the acquirer otherwise.
        LocationInformation.Functions[name] = locationInformation;

        scope = VisitBlock(scope, context.block()).Scope as IFunctionScope;
        if (scope == null) throw new Exception("The scope returned from VisitBlock() was null.");

        // check that all the arguments have been assigned types (if not done manually)
        for (var index = 0; index < scope.Arguments.Count; index++)
        {
            var argument = scope.Arguments[index];
            if (argument.Type != ScratchType.Unknown) continue;

            LocationInformation.Functions.Remove(name);
            DiagnosticReporter.Error((int)ScratchScriptError.ArgumentTypeMustBeSpecifiedManually, context,
                context.typedIdentifier(index), argument.Name);
            return null;
        }

        if (scope.ReturnType == ScratchType.Unknown) scope.ReturnType = ScratchType.Void;

        Exports.Functions[name] = scope;
        return null;
    }

    public override TypedValue? VisitReturnStatement(ScratchScriptParser.ReturnStatementContext context)
    {
        // return must be used in a function context
        if (_scope is not IFunctionScope function)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ReturnUsedInNonFunctionContext, context, context);
            return null;
        }

        // an expression can be null if it returns void ("return;" vs "return 1;")
        var expression = context.expression() != null ? (ExpressionValue?)Visit(context.expression()) : null;
        if (context.expression() != null && expression == null)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression());
            return null;
        }

        // ICE handling
        if (expression != null && expression.Type == ScratchType.Unknown)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ReturnWithExpressionOfUnknownType, context,
                context.expression());
            return null;
        }

        // the function return may have been set earlier, so the expression must be checked if that's the case
        var expressionType = expression?.Type ?? ScratchType.Void;
        if (function.ReturnType != ScratchType.Unknown && function.ReturnType != expressionType)
        {
            var typeSetterContext = LocationInformation.Functions[function.FunctionName].ReturnTypeSetter;

            DiagnosticReporter.Error((int)ScratchScriptError.TypeMismatch, context, context.expression(),
                function.ReturnType, expressionType);
            DiagnosticReporter.Note((int)ScratchScriptNote.ReturnTypeSetAt, typeSetterContext, typeSetterContext);
            return null;
        }

        // or if the function's return type is currently unknown, make it the expression's type
        if (function.ReturnType == ScratchType.Unknown) function.ReturnType = expressionType;

        LocationInformation.Functions[function.FunctionName] = LocationInformation.Functions[function.FunctionName] with
        {
            ReturnTypeSetter = context
        };
        _functionHandler.HandleFunctionExit(ref _scope, expression);
        return null;
    }
}
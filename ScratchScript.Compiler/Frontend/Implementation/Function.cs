using ScratchScript.Compiler.Backend.Information;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor
{
    private int _functionDeclarationCount;

    private IrFunctionNode? FindFunction(string name, IEnumerable<ScratchType> signature)
    {
        return Exports.Functions.Values.FirstOrDefault(func =>
            func.FunctionScope.FunctionName == name &&
            func.FunctionScope.Arguments.Select(arg => arg.Type).SequenceEqual(signature));
    }

    /* TODO: perhaps change this into a reflection-based automated thing?
     like [NativeFunction] public IrNode RawStatement(string opcode, JsonObject data) => {...}
     i guess it's ok if there are only two functions like this...
     */
    private IrNode? HandleSpecialFunctionCall(string name, List<IrExpressionNode> arguments)
    {
        switch (name)
        {
            // __raw(opcode, data)
            case ReservedNames.RawStatementFunction:
            {
                if (arguments.Count != 2) throw new Exception();
                if (DetermineExpressionType(arguments[0]) != ScratchType.String) throw new Exception();
                if (DetermineExpressionType(arguments[1]) != ScratchType.Object) throw new Exception();
                return new IrCallFunctionCommandNode(ReservedNames.RawStatementFunction,
                    new Dictionary<string, IrExpressionNode>
                    {
                        { "opcode", arguments[0] },
                        { "data", arguments[1] }
                    });
            }
            // __raw_expr(opcode, data, type)
            case ReservedNames.RawExpressionFunction:
            {
                if (arguments.Count != 3) throw new Exception();
                if (DetermineExpressionType(arguments[0]) != ScratchType.String) throw new Exception();
                if (DetermineExpressionType(arguments[1]) != ScratchType.Object) throw new Exception();
                // type must be known at compile time
                if (arguments[2] is not IrConstantExpressionNode type ||
                    DetermineExpressionType(type) != ScratchType.String) throw new Exception();
                if (ScratchType.FromString((string)type.Value.Value!) == null) throw new Exception();
                return new IrCallFunctionCommandNode(ReservedNames.RawExpressionFunction,
                    new Dictionary<string, IrExpressionNode>
                    {
                        { "opcode", arguments[0] },
                        { "data", arguments[1] },
                        { "type", arguments[2] }
                    });
            }
            default: return null;
        }
    }

    private IrNode? HandleFunctionCall(
        ScratchScriptParser.FunctionCallStatementContext context)
    {
        if (_scope is null) return null;

        var name = context.Identifier().GetText();
        var arguments = new List<IrExpressionNode>();
        foreach (var arg in context.functionArgument())
        {
            if (Visit(arg.expression()) is not IrExpressionNode expression)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, arg.expression());
                return null;
            }

            arguments.Add(expression);
        }

        // strict name check (without signature)
        if (ReservedNames.GlobalCallableFunctions.Contains(name)) return HandleSpecialFunctionCall(name, arguments);
        if (!Exports.Functions.ContainsKey(name))
        {
            DiagnosticReporter.Error((int)ScratchScriptError.NoFunctionsWithNameAreDefined, context,
                context.Identifier(), name);
            return null;
        }

        var signature = arguments.Select(DetermineExpressionType).ToList();
        var function = FindFunction(name, signature);

        if (function == null)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.NoFunctionWithMatchingSignatureDefined, context, context,
                StringExtensions.GetFunctionSignatureString(name, signature));
            return null;
        }

        // todo: add support for named arguments later (involves combinations and permutations and stuff)
        var namedArguments =
            arguments.ToDictionary(arg => function.FunctionScope.Arguments[arguments.IndexOf(arg)].Name, arg => arg);
        return new IrCallFunctionCommandNode(name, namedArguments);
    }

    public override IrNode? VisitFunctionCallStatement(ScratchScriptParser.FunctionCallStatementContext context)
    {
        return HandleFunctionCall(context);
    }

    public override IrNode? VisitFunctionCallExpression(ScratchScriptParser.FunctionCallExpressionContext context)
    {
        var node = HandleFunctionCall(context.functionCallStatement());
        return node is not IrCallFunctionCommandNode callNode
            ? null
            : new IrFunctionCallExpressionNode(callNode.Function, callNode.Arguments);
    }

    public override IrNode? VisitFunctionDeclarationStatement(
        ScratchScriptParser.FunctionDeclarationStatementContext context)
    {
        var name = context.Identifier().GetText();

        // verify that the name can be used
        // TODO: this technically should be implemented by doing function overloading
        // but for now, basic functions will do. implement it when functions are stable though!
        if (RequireIdentifierUnclaimedOrFail(name, context, context.Identifier())) return null;

        var scope = new FunctionScope
        {
            Id = _functionDeclarationCount.ToString(),
            FunctionName = name,
            ReturnType = ScratchType.Unknown
        };
        _functionDeclarationCount++;

        var locationInformation = new FunctionLocationInformation
        {
            DefinitionContext = context,
            FunctionNameIdentifier = context.Identifier(),
            ArgumentInformation = []
        };

        if (context.type() != null)
        {
            locationInformation.ReturnTypeSetter = context.type();
            var type = ScratchType.FromString(context.type().GetText());
            // TODO: "expected type" diagnostic
            if (type == null) throw new Exception();
            scope.ReturnType = type;
        }

        // register the arguments before parsing the block
        foreach (var identifier in context.typedIdentifier())
        {
            var argumentName = identifier.Identifier().GetText();
            var argumentType = ScratchType.Unknown;

            // if the type is specified
            if (identifier.type() != null)
                argumentType = ScratchType.FromString(identifier.type().GetText());

            if (RequireIdentifierUnclaimedOrFail(argumentName, context, identifier.Identifier())) return null;

            // all functions are top-level (for now) so the scope depth will always be 0
            scope.Arguments.Add(new ScratchScriptVariable(argumentName,
                argumentType));
            locationInformation.ArgumentInformation[argumentName] = (identifier.Identifier(), identifier.type());
        }

        // set the LocationInformation before visiting the scope as any identifier checks
        // will fail to point the location of the acquirer otherwise.
        Exports.Functions[name] = new IrFunctionNode(false, scope);
        LocationInformation.Functions[name] = locationInformation;

        // process function attributes
        /*for (var index = 0; index < context.attributeStatement().Length; index++)
        {
            var attribute = ProcessAttribute(context.attributeStatement(index), false);
            if (attribute == null)
            {
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context,
                    context.attributeStatement(index));
                return null;
            }

            VisitInScope(scope,
                () =>
                {
                    Target.Attribute.ProcessFunctionAttribute(ref _scope!, attribute.Value.Name,
                        attribute.Value.Values);
                });
        }*/

        scope = VisitBlock(scope, context.block()).Scope as FunctionScope;
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

        // TODO: handle warp attribute
        Exports.Functions[name] = new IrFunctionNode(false, scope);
        return Exports.Functions[name];
    }

    public override IrNode? VisitReturnStatement(ScratchScriptParser.ReturnStatementContext context)
    {
        // return must be used in a function context
        var closestFunctionScope = _scope;
        while (closestFunctionScope != null && closestFunctionScope is not FunctionScope)
            closestFunctionScope = closestFunctionScope.ParentScope;
        if (closestFunctionScope is not FunctionScope function)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ReturnUsedInNonFunctionContext, context, context);
            return null;
        }

        // an expression can be null if the function returns void ("return;" vs "return 1;")
        var expression = context.expression() != null ? (IrExpressionNode?)Visit(context.expression()) : null;
        if (context.expression() != null && expression == null)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        // ICE handling
        if (expression != null && DetermineExpressionType(expression) == ScratchType.Unknown)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ReturnWithExpressionOfUnknownType, context,
                context.expression());
            return null;
        }

        // the function return may have been set earlier, so the expression must be checked if that's the case
        var expressionType = expression != null ? DetermineExpressionType(expression) : ScratchType.Void;
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
        return new IrFunctionReturnCommandNode(expression);
    }

    public override IrNode? VisitMemberFunctionCallStatement(
        ScratchScriptParser.MemberFunctionCallStatementContext context)
    {
        throw new NotImplementedException();
    }

    public override IrNode? VisitMemberFunctionCallExpression(
        ScratchScriptParser.MemberFunctionCallExpressionContext context)
    {
        throw new NotImplementedException();
    }
}
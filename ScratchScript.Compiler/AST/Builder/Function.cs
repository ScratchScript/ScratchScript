using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor
{
    private int _functionDeclarationCount;

    private IrFunctionNode? FindFunction(string name, IEnumerable<ScratchType> signature)
    {
        return Enumerable.FirstOrDefault<IrFunctionNode>(Exports.Functions.Values, func =>
            func.FunctionScope.FunctionName == name &&
            Enumerable.Select<ScratchScriptVariable, ScratchType>(func.FunctionScope.Arguments, arg => arg.Type).SequenceEqual(signature));
    }

    /* TODO: perhaps change this into a reflection-based automated thing?
     like [NativeFunction] public IrNode RawStatement(string opcode, JsonObject data) => {...}
     i guess it's ok if there are only two functions like this...
     */
    private IrNode? HandleSpecialFunctionCall(string name, List<(string?, IrExpressionNode)> arguments)
    {
        switch (name)
        {
            // __raw(opcode, data)
            case ReservedNames.RawStatementFunction:
            {
                if (arguments.Count != 2) throw new Exception();
                /*if (DetermineExpressionType(arguments[0]) != ScratchType.String) throw new Exception();
                if (DetermineExpressionType(arguments[1]) != ScratchType.Object) throw new Exception();*/
                return new IrCallFunctionCommandNode(ReservedNames.RawStatementFunction,
                [
                    ("opcode", arguments[0].Item2),
                    ("data", arguments[1].Item2)
                ]);
            }
            // __raw_expr(opcode, data, type)
            case ReservedNames.RawExpressionFunction:
            {
                if (arguments.Count != 3) throw new Exception();
                /*if (DetermineExpressionType(arguments[0]) != ScratchType.String) throw new Exception();
                if (DetermineExpressionType(arguments[1]) != ScratchType.Object) throw new Exception();*/
                // type must be known at compile time
                /*if (arguments[2] is not IrConstantExpressionNode type ||
                    DetermineExpressionType(type) != ScratchType.String) throw new Exception();
                if (ScratchType.FromString((string)type.Value.Value!) == null) throw new Exception();*/
                return new IrCallFunctionCommandNode(ReservedNames.RawExpressionFunction,
                [
                    ("opcode", arguments[0].Item2),
                    ("data", arguments[1].Item2),
                    ("type", arguments[2].Item2)
                ]);
            }
            default: return null;
        }
    }

    private IrNode? HandleFunctionCall(
        ScratchScriptParser.FunctionCallStatementContext context)
    {
        if (_scope is null) return null;

        var name = context.Identifier().GetText();
        var arguments = new List<(string?, IrExpressionNode)>();
        foreach (var argument in context.functionArgument())
        {
            var argumentName = argument.Identifier()?.GetText();
            if (Visit(argument.expression()) is not IrExpressionNode expression)
            {
                DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, argument.expression());
                return null;
            }

            arguments.Add((argumentName, expression));
        }

        // strict name check (without signature)
        if (ReservedNames.GlobalCallableFunctions.Contains(name))
            return HandleSpecialFunctionCall(name, arguments).WithContext(context);
        if (!Exports.Functions.ContainsKey(name))
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.NoFunctionsWithNameAreDefined, context,
                context.Identifier(), name);
            return null;
        }

        // todo: add support for named arguments later (involves combinations and permutations and stuff)
        return new IrCallFunctionCommandNode(name, arguments);
    }

    public override IrNode? VisitFunctionCallStatement(ScratchScriptParser.FunctionCallStatementContext context)
    {
        return HandleFunctionCall(context).WithContext(context);
    }

    public override IrNode? VisitFunctionCallExpression(ScratchScriptParser.FunctionCallExpressionContext context)
    {
        var node = HandleFunctionCall(context.functionCallStatement());
        return node is not IrCallFunctionCommandNode callNode
            ? null
            : new IrFunctionCallExpressionNode(callNode.Function, callNode.Arguments).WithContext(context);
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
                argumentType = ScratchType.FromString(identifier.type().GetText()) ?? ScratchType.Unknown;

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
                DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context,
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

        // TODO: handle warp attribute
        Exports.Functions[name] = new IrFunctionNode(false, scope).WithContext(context);
        return Exports.Functions[name];
    }

    public override IrNode? VisitReturnStatement(ScratchScriptParser.ReturnStatementContext context)
    {
        // return must be used in a function context
        var closestFunctionScope = _scope?.GetClosestFunctionScope();
        if (closestFunctionScope is not FunctionScope function)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ReturnUsedInNonFunctionContext, context, context);
            return null;
        }

        // an expression can be null if the function returns void ("return;" vs "return 1;")
        var expression = context.expression() != null ? (IrExpressionNode?)Visit(context.expression()) : null;
        if (context.expression() != null && expression == null)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, context.expression());
            return null;
        }

        LocationInformation.Functions[function.FunctionName] = LocationInformation.Functions[function.FunctionName] with
        {
            ReturnTypeSetter = context
        };
        return new IrFunctionReturnCommandNode(expression).WithContext(context);
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
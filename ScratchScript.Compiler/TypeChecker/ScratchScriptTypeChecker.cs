using Antlr4.Runtime;
using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.TypeChecker;

public class ScratchScriptTypeChecker : IrRewriter
{
    public bool Success { get; private set; } = true;

    public override IrNode VisitProgram(IrProgramNode node)
    {
        ProgramNode = node;
        DiagnosticReporter.Instance.Reported += message =>
        {
            if (message.Kind == DiagnosticMessageKind.Error) Success = false;
        };

        return base.VisitProgram(node);
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        var variable = CurrentScope?.GetVariable(node.Variable);
        var expression = (IrExpressionNode)Visit(node.Expression);
        if (variable == null) throw new Exception();

        if (!MustMatchTypeOrFail(expression, variable.Type, node.Context, expression.Context))
            variable.Type = expression.InferredType;
        return node with { Expression = expression };
    }

    public override IrNode VisitWhileCommand(IrWhileCommandNode node)
    {
        var condition = (IrExpressionNode)Visit(node.Condition);
        MustMatchTypeOrFail(condition, ScratchType.Boolean, node.Context, node.Condition.Context);
        return (IrWhileCommandNode)base.VisitWhileCommand(node) with { Condition = condition };
    }

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node)
    {
        var times = (IrExpressionNode)Visit(node.Times);
        MustMatchTypeOrFail(times, ScratchType.Number, node.Context, node.Times.Context);
        return (IrRepeatCommandNode)base.VisitRepeatCommand(node) with { Times = times };
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        if (node.Context is not ScratchScriptParser.FunctionCallStatementContext context)
            throw new Exception(
                "Expected a FunctionCallStatementContext as the context of IrCallFunctionCommandNode");
        var (function, visitedArguments) = HandleFunctionCall(context, node.Arguments);

        if (function == null) return node;
        return node with { Arguments = visitedArguments };
    }

    public override IrNode VisitRawCommand(IrRawCommandNode node)
        => node;

    // TODO: check expression type when lists are implemented properly?
    public override IrNode VisitPushCommand(IrPushCommand node)
        => node;

    public override IrNode VisitPushAtCommand(IrPushAtCommand node)
    {
        var where = (IrExpressionNode)Visit(node.Where);
        MustMatchTypeOrFail(where, ScratchType.Number, node.Context, node.Where.Context);
        return (IrPushAtCommand)base.VisitPushAtCommand(node) with { Where = where };
    }

    public override IrNode VisitPopCommand(IrPopCommand node)
        => node;

    public override IrNode VisitPopAtCommand(IrPopAtCommand node)
    {
        var where = (IrExpressionNode)Visit(node.Where);
        MustMatchTypeOrFail(where, ScratchType.Number, node.Context, node.Where.Context);
        return (IrPopAtCommand)base.VisitPopAtCommand(node) with { Where = where };
    }

    public override IrNode VisitPopAllCommand(IrPopAllCommand node)
        => node;

    public override IrNode VisitIfCommand(IrIfCommandNode node)
    {
        var condition = (IrExpressionNode)Visit(node.Condition);
        MustMatchTypeOrFail(condition, ScratchType.Boolean, node.Context, node.Condition.Context);
        return (IrIfCommandNode)base.VisitIfCommand(node) with { Condition = condition };
    }

    public override IrNode VisitConstantExpression(IrConstantExpressionNode node)
        => node.WithInferredType(node.Value.Type);

    public override IrNode VisitLocalVariableIdentifierExpression(IrLocalVariableIdentifierExpressionNode node)
        => node.WithInferredType(CurrentScope?.GetVariable(node.Name)?.Type ?? ScratchType.Unknown);

    public override IrNode VisitGlobalVariableIdentifierExpression(IrGlobalVariableIdentifierExpressionNode node)
        => throw new NotImplementedException();

    public override IrNode VisitGlobalListIdentifierExpression(IrGlobalListIdentifierExpressionNode node)
        => throw new NotImplementedException();

    public override IrNode VisitParenthesizedExpression(IrParenthesizedExpressionNode node)
        => node.WithInferredType(((IrExpressionNode)Visit(node.Expression)).InferredType);

    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        var left = (IrExpressionNode)Visit(node.Left);
        var right = (IrExpressionNode)Visit(node.Right);

        if (left.InferredType == ScratchType.String && right.InferredType == ScratchType.String)
            return new IrBinaryExpressionNode(IrBinaryOperator.Join, left, right).WithInferredType(ScratchType.String);

        var expectedType = node.Operator switch
        {
            >= IrBinaryOperator.Add and < IrBinaryOperator.Join => ScratchType.Number,
            IrBinaryOperator.Join => ScratchType.String,
            >= IrBinaryOperator.And and <= IrBinaryOperator.Xor => ScratchType.Boolean,
            IrBinaryOperator.Equal or IrBinaryOperator.NotEqual => left.InferredType,
            >= IrBinaryOperator.LessThan and <= IrBinaryOperator.GreaterOrEqualTo => ScratchType.Number,
            _ => throw new ArgumentOutOfRangeException()
        };

        var outputType = node.Operator switch
        {
            >= IrBinaryOperator.Add and < IrBinaryOperator.Join => ScratchType.Number,
            IrBinaryOperator.Join => ScratchType.String,
            >= IrBinaryOperator.And => ScratchType.Boolean,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (MustMatchTypeOrFail(left, expectedType, node.Context, left.Context)) return node;
        if (MustMatchTypeOrFail(right, expectedType, node.Context, right.Context)) return node;
        return (node with { Left = left, Right = right }).WithInferredType(outputType);
    }

    public override IrNode VisitUnaryExpression(IrUnaryExpressionNode node)
    {
        var operand = (IrExpressionNode)Visit(node.Operand);
        var type = node.Operator == IrUnaryOperator.Not ? ScratchType.Boolean : ScratchType.Number;

        MustMatchTypeOrFail(operand, type,
            node.Context, node.Operand.Context);

        return (node with { Operand = operand }).WithInferredType(type);
    }

    public override IrNode VisitShadowExpression(IrShadowExpressionNode node)
        => node.WithInferredType(node.ExpectedType ?? ScratchType.Unknown);

    public override IrNode VisitComplexExpression(IrComplexExpressionNode node)
        => node.WithInferredType(((IrExpressionNode)Visit(node.Expression)).InferredType);

    public override IrNode VisitObjectLiteralExpression(IrObjectLiteralExpressionNode node)
        => node.WithInferredType(ScratchType.Object);

    public override IrNode VisitTernaryExpression(IrTernaryExpressionNode node)
    {
        var condition = (IrExpressionNode)Visit(node.Condition);
        var trueValue = (IrExpressionNode)Visit(node.TrueValue);
        var falseValue = (IrExpressionNode)Visit(node.FalseValue);

        MustMatchTypeOrFail(condition, ScratchType.Boolean, node.Context, node.Condition.Context);
        MustMatchTypeOrFail(falseValue, trueValue.InferredType, node.Context, node.FalseValue.Context);

        return (node with { Condition = condition, TrueValue = trueValue, FalseValue = falseValue }).WithInferredType(
            trueValue.InferredType);
    }

    public override IrNode VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
        => node.WithInferredType(CurrentScope?.GetArgument(node.Name)?.Type ?? ScratchType.Unknown);

    public override IrNode VisitStackPointerExpressionNode(IrStackPointerExpressionNode node)
        => node.WithInferredType(ScratchType.Unknown);

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        if (node.Context is not ScratchScriptParser.FunctionCallExpressionContext context)
            throw new Exception(
                "Expected a FunctionCallExpressionContext as the context of IrFunctionCallExpressionNode");
        var (function, visitedArguments) = HandleFunctionCall(context.functionCallStatement(), node.Arguments);

        if (function == null) return node;
        // TODO: make this a diagnostic error
        if (function.FunctionScope.ReturnType == ScratchType.Unknown)
            throw new Exception("Function cannot return unknown");

        return node with { InferredType = function.FunctionScope.ReturnType, Arguments = visitedArguments };
    }

    public override IrNode VisitFunctionReturnCommandNode(IrReturnCommandNode node)
    {
        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope == null) throw new Exception();

        var value = node.ReturnValue != null ? (IrExpressionNode)Visit(node.ReturnValue) : null;
        if (value != null) MustNotMatchTypeOrFail(value, ScratchType.Unknown, node.Context, node.ReturnValue!.Context);
        var returnType = value != null ? value.InferredType : ScratchType.Void;

        closestFunctionScope.ReturnType = returnType;
        return node with { ReturnValue = value };
    }

    private static bool MustMatchTypeOrFail(IrExpressionNode node, ScratchType expected, ParserRuleContext ownContext,
        ParserRuleContext ownSource)
    {
        if (expected == ScratchType.Unknown) return false;
        if (node.InferredType == expected) return false;

        DiagnosticReporter.Instance.Error((int)ScratchScriptError.TypeMismatch, ownContext, ownSource, expected,
            node.InferredType);
        return true;
    }

    private static bool MustNotMatchTypeOrFail(IrExpressionNode node, ScratchType avoided, ParserRuleContext ownContext,
        ParserRuleContext ownSource)
    {
        if (avoided == ScratchType.Unknown) return false;
        if (node.InferredType != avoided) return false;

        // TODO: change TypeMismatch to new TypeMustNotMatch
        DiagnosticReporter.Instance.Error((int)ScratchScriptError.TypeMismatch, ownContext, ownSource, avoided,
            node.InferredType);
        return true;
    }

    private (IrFunctionNode? Function, IEnumerable<IrExpressionNode> VisitedArguments) HandleFunctionCall(
        ScratchScriptParser.FunctionCallStatementContext context,
        IEnumerable<IrExpressionNode> arguments)
    {
        var name = context.Identifier().GetText();
        if (ProgramNode.Functions.All(f => f.FunctionScope.FunctionName != name) &&
            !ReservedNames.GlobalCallableFunctions.Contains(name))
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.NoFunctionsWithNameAreDefined, context,
                context.Identifier(), name);
            return (null, arguments);
        }

        var visitedArguments = arguments.Select(Visit).OfType<IrExpressionNode>().ToList();
        var function = FindFunction(name, visitedArguments);

        if (function == null)
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.NoFunctionWithMatchingSignatureDefined, context,
                context,
                StringExtensions.GetFunctionSignatureString(name, visitedArguments.Select(a => a.InferredType)));

        return (function, visitedArguments);
    }

    private IrFunctionNode? FindFunction(string name, IEnumerable<IrExpressionNode> arguments)
    {
        if (ReservedNames.GlobalCallableFunctions.Contains(name))
        {
            // TODO: this is quite a hack but idk
            return name switch
            {
                ReservedNames.RawStatementFunction => new IrFunctionNode(true,
                    new FunctionScope { ReturnType = ScratchType.Void }),
                ReservedNames.RawExpressionFunction => new IrFunctionNode(true,
                    new FunctionScope { ReturnType = arguments.ElementAt(2).InferredType }),
                _ => throw new ArgumentOutOfRangeException(nameof(name))
            };
        }

        return ProgramNode.Functions.FirstOrDefault(func =>
            func.FunctionScope.FunctionName == name &&
            func.FunctionScope.Arguments.Select(arg => arg.Type)
                .SequenceEqual(arguments.Select(a => a.InferredType)));
    }
}
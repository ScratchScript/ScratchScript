using Antlr4.Runtime;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.TypeChecker;

public class ScratchScriptTypeChecker : IrRewriter
{
    private Scope? _scope;
    private IrProgramNode _program;

    private IEnumerable<IrFunctionNode> Functions => _program.Blocks.OfType<IrFunctionNode>();
    public bool Success { get; private set; } = true;

    public override IrNode VisitProgram(IrProgramNode node)
    {
        _program = node;
        DiagnosticReporter.Instance.Reported += message =>
        {
            if (message.Kind == DiagnosticMessageKind.Error) Success = false;
        };

        return base.VisitProgram(node);
    }

    public override IrNode VisitBlock(IrBlockNode node)
    {
        var previousScope = _scope;
        _scope = node.Scope;

        var result = (IrBlockNode)base.VisitBlock(node);
        _scope = previousScope;
        return result;
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        var variable = _scope?.GetVariable(node.Variable);
        var expression = (IrExpressionNode)Visit(node.Expression);
        if (variable == null) throw new Exception();

        if (!MustMatchTypeOrFail(expression, variable.Type, node.Context, expression.Context))
            variable.Type = expression.InferredType;
        return node;
    }

    public override IrNode VisitWhileCommand(IrWhileCommandNode node)
    {
        return base.VisitWhileCommand(node);
    }

    public override IrNode VisitRepeatCommand(IrRepeatCommandNode node)
    {
        return base.VisitRepeatCommand(node);
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        return base.VisitCallFunctionCommand(node);
    }

    public override IrNode VisitRawCommand(IrRawCommandNode node)
    {
        return base.VisitRawCommand(node);
    }

    public override IrNode VisitPushCommand(IrPushCommand node)
    {
        return base.VisitPushCommand(node);
    }

    public override IrNode VisitPushAtCommand(IrPushAtCommand node)
    {
        return base.VisitPushAtCommand(node);
    }

    public override IrNode VisitPopCommand(IrPopCommand node)
    {
        return base.VisitPopCommand(node);
    }

    public override IrNode VisitPopAtCommand(IrPopAtCommand node)
    {
        return base.VisitPopAtCommand(node);
    }

    public override IrNode VisitPopAllCommand(IrPopAllCommand node)
    {
        return base.VisitPopAllCommand(node);
    }

    public override IrNode VisitIfCommand(IrIfCommandNode node)
    {
        return base.VisitIfCommand(node);
    }

    public override IrNode VisitConstantExpression(IrConstantExpressionNode node)
        => node.WithInferredType(node.Value.Type);

    public override IrNode VisitLocalVariableExpression(IrLocalVariableIdentifierExpressionNode node)
        => node.WithInferredType(_scope?.GetVariable(node.Name)?.Type ?? ScratchType.Unknown);

    public override IrNode VisitGlobalVariableExpression(IrGlobalVariableIdentifierExpressionNode node)
        => throw new NotImplementedException();

    public override IrNode VisitGlobalListExpression(IrGlobalListIdentifierExpressionNode node)
        => throw new NotImplementedException();

    public override IrNode VisitParenthesizedExpression(IrParenthesizedExpressionNode node)
        => node.WithInferredType(((IrExpressionNode)Visit(node.Expression)).InferredType);

    public override IrNode VisitBinaryExpression(IrBinaryExpressionNode node)
    {
        var left = (IrExpressionNode)Visit(node.Left);
        var right = (IrExpressionNode)Visit(node.Right);

        if (left.InferredType == ScratchType.String && right.InferredType == ScratchType.String)
            return new IrBinaryExpressionNode(IrBinaryOperator.Join, left, right).WithInferredType(ScratchType.String);

        if (MustMatchTypeOrFail(left, ScratchType.Number, node.Context, left.Context)) return node;
        if (MustMatchTypeOrFail(right, ScratchType.Number, node.Context, right.Context)) return node;
        return (node with { Left = left, Right = right }).WithInferredType(ScratchType.Number);
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
        => node.WithInferredType(_scope?.GetArgument(node.Name)?.Type ?? ScratchType.Unknown);

    public override IrNode VisitStackPointerExpressionNode(IrStackPointerExpressionNode node)
        => node.WithInferredType(ScratchType.Unknown);

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        return base.VisitFunctionCallExpressionNode(node);
    }

    public override IrNode VisitFunctionReturnCommandNode(IrFunctionReturnCommandNode node)
    {
        var closestFunctionScope = _scope?.GetClosestFunctionScope();
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
}
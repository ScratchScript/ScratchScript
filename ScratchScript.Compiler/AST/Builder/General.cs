using System.Globalization;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.AST.Builder;

public partial class ScratchScriptVisitor : ScratchScriptParserBaseVisitor<IrNode?>
{
    private Scope? _scope;

    public ScratchScriptVisitor()
    {
        DiagnosticReporter.Instance.Reported += message =>
        {
            if (message.Kind == DiagnosticMessageKind.Error) Success = false;
        };
    }

    public bool Success { get; private set; } = true;
    public DiagnosticLocationStorage LocationInformation { get; } = new();
    public ExportsStorage Exports { get; } = new();

    public override IrNode? VisitProgram(ScratchScriptParser.ProgramContext context)
    {
        var blocks = context.topLevelStatement().Select(Visit).Cast<IrBlockNode>().ToList();
        return new IrProgramNode(blocks, []).WithContext(context);
    }

    public override IrNode? VisitConstant(ScratchScriptParser.ConstantContext context)
    {
        if (context.Number() is { } n)
            return new IrConstantExpressionNode(TypedValue.Number(double.Parse(n.GetText(),
                CultureInfo.InvariantCulture)));
        if (context.String() is { } s)
            return new IrConstantExpressionNode(TypedValue.String(s.GetText()[1..^1]));
        if (context.boolean() is { } b)
            return new IrConstantExpressionNode(TypedValue.Boolean(b.GetText() == "true"));
        if (context.Color() is { } c)
            return new IrConstantExpressionNode(TypedValue.Color(c.GetText()[1..]));
        return null;
    }

    public override IrNode? VisitObjectLiteralExpression(ScratchScriptParser.ObjectLiteralExpressionContext context)
    {
        var values = new Dictionary<string, IrExpressionNode>();
        foreach (var property in context.objectProperty())
        {
            var key = property.propertyKey().Identifier()?.GetText() ??
                      property.propertyKey().String()!.GetText()![1..^1];
            if (Visit(property.expression()) is not IrExpressionNode value)
            {
                DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedExpression, context, property.expression());
                return null;
            }

            values[key] = value;
        }

        return new IrConstantExpressionNode(TypedValue.Object(values)).WithContext(context);
    }

    public override IrNode? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
    {
        var expression = Visit(context.expression());
        return expression == null
            ? null
            : new IrParenthesizedExpressionNode((IrExpressionNode)expression).WithContext(context);
    }

    public override IrNode? VisitConstantExpression(ScratchScriptParser.ConstantExpressionContext context)
    {
        if (Visit(context.constant()) is not { } value)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.constant());
            return null;
        }

        return value.WithContext(context);
    }

    public override IrNode? VisitInterpolatedString(ScratchScriptParser.InterpolatedStringContext context)
    {
        IrExpressionNode? result = null;

        foreach (var part in context.interpolatedStringPart())
            if (part.Text() != null)
            {
                var value = new IrConstantExpressionNode(new TypedValue(part.Text().GetText(), ScratchType.String));
                result = result == null
                    ? value
                    : new IrBinaryExpressionNode(IrBinaryOperator.Join, result, value);
            }
            else if (part.expression() != null)
            {
                if (Visit(part.expression()) is IrExpressionNode partValue)
                    result = result == null
                        ? partValue
                        : new IrBinaryExpressionNode(IrBinaryOperator.Join, result, partValue).WithContext(
                            part.expression());
                else
                    throw new NotImplementedException("Diagnostic error");
            }

        return result.WithContext(context);
    }

    public override IrNode? VisitBlock(ScratchScriptParser.BlockContext context)
    {
        return VisitBlock(new Scope(), context);
    }

    private IrBlockNode VisitBlock(Scope scope, ScratchScriptParser.BlockContext context)
    {
        if (_scope != null)
        {
            scope.ParentScope = _scope;
            scope.Depth = _scope.Depth + 1;
        }

        _scope = scope;

        foreach (var lineContext in context.line())
        {
            if (VisitLine(lineContext) is not { } value) continue;
            switch (value)
            {
                case IrBlockNode blockNode:
                {
                    scope.Body.AddRange(blockNode.Scope.Body);
                    break;
                }
                case IrCommandNode commandNode:
                {
                    scope.Body.Add(commandNode);
                    break;
                }
            }
        }

        _scope = scope.ParentScope;
        return new IrBlockNode(scope).WithContext(context);
    }

    public override IrNode? VisitLine(ScratchScriptParser.LineContext context)
    {
        return context.statement() != null
            ? VisitStatement(context.statement()).WithContext(context.statement())
            : base.VisitLine(context).WithContext(context);
    }

    public override IrNode? VisitTernaryExpression(ScratchScriptParser.TernaryExpressionContext context)
    {
        if (Visit(context.expression(0)) is not IrExpressionNode condition)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(0));
            return null;
        }

        if (Visit(context.expression(1)) is not IrExpressionNode trueValue)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(1));
            return null;
        }

        if (Visit(context.expression(2)) is not IrExpressionNode falseValue)
        {
            DiagnosticReporter.Instance.Error((int)ScratchScriptError.ExpectedNonNull, context, context.expression(2));
            return null;
        }

        return new IrTernaryExpressionNode(condition, trueValue, falseValue).WithContext(context);
    }
}
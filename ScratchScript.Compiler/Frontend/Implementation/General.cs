using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor : ScratchScriptParserBaseVisitor<IrNode?>
{
    private Scope? _scope;

    public ScratchScriptVisitor()
    {
        DiagnosticReporter.Reported += message =>
        {
            if (message.Kind == DiagnosticMessageKind.Error) Success = false;
        };
    }

    public bool Success { get; private set; } = true;
    public DiagnosticReporter DiagnosticReporter { get; } = new();
    public DiagnosticLocationStorage LocationInformation { get; } = new();
    public ExportsStorage Exports { get; } = new();

    public override IrNode? VisitProgram(ScratchScriptParser.ProgramContext context)
    {
        var blocks = context.topLevelStatement().Select(Visit).Cast<IrBlockNode>().ToList();
        return new IrProgramNode(blocks, []);
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
                DiagnosticReporter.Error((int)ScratchScriptError.ExpectedExpression, context, property.expression());
                return null;
            }

            values[key] = value;
        }

        return new IrConstantExpressionNode(TypedValue.Object(values));
    }

    public override IrNode? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
    {
        var expression = Visit(context.expression());
        return expression == null ? null : new IrParenthesizedExpressionNode((IrExpressionNode)expression);
    }

    public override IrNode? VisitConstantExpression(ScratchScriptParser.ConstantExpressionContext context)
    {
        if (Visit(context.constant()) is not { } value)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.constant());
            return null;
        }

        return value;
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
                        : new IrBinaryExpressionNode(IrBinaryOperator.Join, result, partValue);
                else
                    throw new NotImplementedException("Diagnostic error");
            }

        return result;
    }

    public override IrNode? VisitBlock(ScratchScriptParser.BlockContext context)
    {
        var scope = new Scope();

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
        return new IrBlockNode(scope);
    }

    public override IrNode? VisitLine(ScratchScriptParser.LineContext context)
        => context.statement() != null ? VisitStatement(context.statement()) : base.VisitLine(context);
}
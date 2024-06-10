using System.Globalization;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.GeneratedVisitor;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.Implementation;

public partial class ScratchCEVisitor : ScratchCEBaseVisitor<TypedValue?>
{
    private Dictionary<string, TypedValue> Parameters { get; } = [];

    public override TypedValue? VisitProgram(ScratchCEParser.ProgramContext context)
    {
        foreach (var parameterContext in context.parameter())
            VisitParameter(parameterContext);
        return Visit(context.expression());
    }

    public override TypedValue? VisitParameter(ScratchCEParser.ParameterContext context)
    {
        var name = context.Identifier().GetText();
        var value = VisitConstant(context.constant());

        if (value == null) throw new Exception($"Could not parse the parameter value (\"{name}\").");
        if (!Parameters.TryAdd(name, value))
            throw new Exception($"Duplicate parameter definition detected (\"{name}\").");
        return null;
    }

    public override TypedValue? VisitConstant(ScratchCEParser.ConstantContext context)
    {
        if (context.Number() is { } n)
            return new TypedValue(double.Parse(n.GetText(), CultureInfo.InvariantCulture), ScratchType.Number);
        if (context.String() is { } s)
            return new TypedValue(s.GetText()[1..^1], ScratchType.String);
        if (context.Boolean() is { } b)
            return new TypedValue(b.GetText() == "true", ScratchType.Boolean);
        return null;
    }

    public override TypedValue? VisitConstantExpression(ScratchCEParser.ConstantExpressionContext context)
    {
        return Visit(context.constant());
    }

    public override TypedValue VisitIdentifierExpression(ScratchCEParser.IdentifierExpressionContext context)
    {
        var name = context.Identifier().GetText();
        if (!Parameters.TryGetValue(name, out var value)) throw new Exception($"Unknown parameter \"{name}\".");
        return value;
    }

    public override TypedValue? VisitParenthesizedExpression(ScratchCEParser.ParenthesizedExpressionContext context)
    {
        return Visit(context.expression());
    }
}
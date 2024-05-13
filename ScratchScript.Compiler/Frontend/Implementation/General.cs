using System.Globalization;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public partial class ScratchScriptVisitor : ScratchScriptParserBaseVisitor<TypedValue?>
{
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
    public Scope? Scope { get; private set; } = null;

    public string Output
    {
        get
        {
            var sb = new StringBuilder();
            return sb.ToString();
        }
    }

    public override TypedValue? VisitConstant(ScratchScriptParser.ConstantContext context)
    {
        if (context.Number() is { } n)
            return new TypedValue(decimal.Parse(n.GetText(), CultureInfo.InvariantCulture), ScratchType.Number);
        if (context.String() is { } s)
            return new TypedValue(s.GetText(), ScratchType.String);
        if (context.boolean() is { } b)
            return new TypedValue(b.GetText() == "true", ScratchType.Boolean);
        if (context.Color() is { } c)
            return new TypedValue(c.GetText()[1..], ScratchType.Color);
        return null;
    }

    public override TypedValue? VisitConstantExpression(ScratchScriptParser.ConstantExpressionContext context)
    {
        if (Visit(context.constant()) is not { } value)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.constant());
            return null;
        }
        
        return new ExpressionValue(value.Value, value.Type);
    }

    public override TypedValue? VisitRegularType(ScratchScriptParser.RegularTypeContext context)
    {
        if (context.Type() != null)
        {
            var kind = Enum.Parse<ScratchTypeKind>(context.Type().GetText());
            var type = new ScratchType(kind);
            return new TypeDeclarationValue(type);
        }

        if (context.Identifier() != null)
        {
            var name = context.Identifier().GetText();
            if (Exports.Enums.TryGetValue(name, out var type)) return new TypeDeclarationValue(type);
        }

        return null;
    }

    public override TypedValue? VisitListType(ScratchScriptParser.ListTypeContext context)
    {
        if (Visit(context.type()) is not TypeDeclarationValue childType)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context.type(), context.type());
            return null;
        }

        var type = ScratchType.List(childType.Type);
        return new TypeDeclarationValue(type);
    }

    public override TypedValue VisitBlock(ScratchScriptParser.BlockContext context)
    {
        var scope = new Scope();
        if (Scope != null)
        {
            scope.ParentScope = Scope;
            scope.Depth = Scope.Depth + 1;
        }

        Scope = scope;

        foreach (var lineContext in context.line())
        {
            if (VisitLine(lineContext) is not { } value) continue;
            scope.Content.Add(value.ToString());
        }

        Scope = scope.ParentScope;
        return new ScopeValue(scope);
    }
}
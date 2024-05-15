using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Frontend.Targets.Scratch3;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public enum CompilerTarget
{
    Scratch3,
    TurboWarp
}

public record ScratchScriptVisitorSettings(char CommandSeparator = ' ');

public partial class ScratchScriptVisitor : ScratchScriptParserBaseVisitor<TypedValue?>
{
    private IBinaryHandler _binaryHandler = null!;

    private IDataHandler _dataHandler = null!;

    private Scope? _scope;

    private CompilerTarget _target = CompilerTarget.Scratch3;
    private IUnaryHandler _unaryHandler = null!;

    public ScratchScriptVisitor(string source, CompilerTarget target = CompilerTarget.Scratch3)
    {
        Id = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(source))).ToString("N");
        DiagnosticReporter.Reported += message =>
        {
            if (message.Kind == DiagnosticMessageKind.Error) Success = false;
        };
        Target = target;
    }

    public string Id { get; }
    public bool Success { get; private set; } = true;
    public ScratchScriptVisitorSettings Settings { get; set; } = new();
    public DiagnosticReporter DiagnosticReporter { get; } = new();
    public DiagnosticLocationStorage LocationInformation { get; } = new();
    public ExportsStorage Exports { get; } = new();

    public CompilerTarget Target
    {
        get => _target;
        set
        {
            _target = value;
            _dataHandler = value switch
            {
                CompilerTarget.Scratch3 => new Scratch3DataHandler(),
                _ => throw new NotImplementedException()
            };
            _binaryHandler = value switch
            {
                CompilerTarget.Scratch3 => new Scratch3BinaryHandler(Settings.CommandSeparator),
                _ => throw new NotImplementedException()
            };
            _unaryHandler = value switch
            {
                CompilerTarget.Scratch3 => new Scratch3UnaryHandler(),
                _ => throw new NotImplementedException()
            };
        }
    }

    public string Output
    {
        get
        {
            var sb = new StringBuilder();

            foreach (var eventScope in Exports.Events.Values)
                sb.AppendLine(eventScope.ToString(Settings.CommandSeparator));

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
        var scope = Target switch
        {
            CompilerTarget.Scratch3 => new Scratch3Scope(),
            _ => throw new NotImplementedException()
        };

        if (_scope != null)
        {
            scope.ParentScope = _scope;
            scope.Depth = _scope.Depth + 1;
        }

        _scope = scope;

        foreach (var lineContext in context.line())
        {
            if (VisitLine(lineContext) is not { } value) continue;
            scope.Content.Add(value.ToString());
        }

        _scope = scope.ParentScope as Scratch3Scope;
        return new ScopeValue(scope);
    }
}
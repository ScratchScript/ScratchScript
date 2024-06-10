using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ScratchScript.Compiler.Diagnostics;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.GeneratedVisitor;
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

public record ScratchScriptVisitorSettings(char CommandSeparator = ' ', bool UseConstantEvaluation = true);

public partial class ScratchScriptVisitor : ScratchScriptParserBaseVisitor<TypedValue?>
{
    private IScope? _scope;

    private CompilerTarget _target = CompilerTarget.Scratch3;

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
            _functionHandler = value switch
            {
                CompilerTarget.Scratch3 => new Scratch3FunctionHandler(),
                _ => throw new NotImplementedException()
            };
            _conditionalHandler = value switch
            {
                CompilerTarget.Scratch3 => new Scratch3ConditionalHandler(),
                _ => throw new NotImplementedException()
            };
            _attributeHandler = value switch
            {
                CompilerTarget.Scratch3 => new Scratch3AttributeHandler(),
                _ => throw new NotImplementedException()
            };
            _enumHandler = value switch
            {
                CompilerTarget.Scratch3 => new Scratch3EnumHandler(),
                _ => throw new NotImplementedException()
            };
        }
    }

    public string Output
    {
        get
        {
            var sb = new StringBuilder();

            if (Exports.Enums.Count != 0)
            {
                sb.AppendJoin(Settings.CommandSeparator, _enumHandler.ConvertEnumsToBackend(Exports.Enums.Values));
                sb.AppendLine();
                sb.AppendLine();
            }

            var functions = Exports.Functions.Values.Where(function => !function.Inlined).ToList();
            foreach (var functionScope in functions)
                sb.AppendLine(functionScope.ToString(Settings.CommandSeparator));
            if (functions.Count != 0) sb.AppendLine();

            foreach (var eventScope in Exports.Events.Values)
                sb.AppendLine(eventScope.ToString(Settings.CommandSeparator));

            return sb.ToString();
        }
    }

    public override TypedValue? VisitConstant(ScratchScriptParser.ConstantContext context)
    {
        if (context.Number() is { } n)
            return new TypedValue(double.Parse(n.GetText(), CultureInfo.InvariantCulture), ScratchType.Number);
        if (context.String() is { } s)
            return new TypedValue(s.GetText(), ScratchType.String);
        if (context.boolean() is { } b)
            return new TypedValue(b.GetText() == "true", ScratchType.Boolean);
        if (context.Color() is { } c)
            return new TypedValue(c.GetText()[1..], ScratchType.Color);
        return null;
    }

    public override TypedValue? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
    {
        return Visit(context.expression());
    }

    public override TypedValue? VisitConstantExpression(ScratchScriptParser.ConstantExpressionContext context)
    {
        if (Visit(context.constant()) is not { } value)
        {
            DiagnosticReporter.Error((int)ScratchScriptError.ExpectedNonNull, context, context.constant());
            return null;
        }

        return new ExpressionValue(value.Value, value.Type, ContainsIntermediateRepresentation: false);
    }

    public override TypedValue? VisitRegularType(ScratchScriptParser.RegularTypeContext context)
    {
        if (context.Type() != null)
        {
            var kind = Enum.Parse<ScratchTypeKind>(context.Type().GetText().Capitalize());
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
        var scope = CreateDefaultScope();
        return VisitBlock(scope, context);
    }

    public override TypedValue? VisitInterpolatedString(ScratchScriptParser.InterpolatedStringContext context)
    {
        var result = "";
        var dependencies = new List<string>();
        var cleanup = new List<string>();

        foreach (var part in context.interpolatedStringPart())
            if (part.Text() != null)
            {
                var processedText = part.Text().GetText().Surround('"');
                result = string.IsNullOrEmpty(result) ? processedText : $"~ {result} {processedText}";
            }
            else if (part.expression() != null && Visit(part.expression()) is { } partValue)
            {
                result = string.IsNullOrEmpty(result) ? partValue.Value!.ToString() : $"~ {result} {partValue.Value}";
                if (partValue is ExpressionValue expression)
                {
                    dependencies.AddRange(expression.Dependencies ?? []);
                    cleanup.AddRange(expression.Cleanup ?? []);
                }
            }

        return new ExpressionValue(result, ScratchType.String, dependencies, cleanup);
    }

    public override TypedValue VisitIrBlockStatement(ScratchScriptParser.IrBlockStatementContext context)
    {
        var lines = new List<string>();
        foreach (var statement in context.irStatement())
        {
            var result = "";
            var dependencies = new List<string>();
            var cleanup = new List<string>();

            // string handling for ir blocks is different so VisitInterpolatedString cannot be called here.
            foreach (var part in statement.interpolatedString().interpolatedStringPart())
                if (part.Text() != null)
                {
                    result += part.Text().GetText();
                }
                else if (part.expression() != null && Visit(part.expression()) is { } partValue)
                {
                    result += partValue.Value!.ToString();
                    if (partValue is ExpressionValue expression)
                    {
                        dependencies.AddRange(expression.Dependencies ?? []);
                        cleanup.AddRange(expression.Cleanup ?? []);
                    }
                }

            if (statement.Return() != null && _scope is IFunctionScope functionScope)
            {
                // TODO: how is the type determined here?
                var returnValue = new ExpressionValue(result, functionScope.ReturnType, dependencies, cleanup);

                if (functionScope.Inlined)
                    functionScope.InlinedReturnValue = returnValue;
                else
                    _functionHandler.HandleFunctionExit(_scope, returnValue);
            }
            else if (statement.Return() == null)
            {
                lines.AddRange([..dependencies, result, ..cleanup]);
            }
        }

        return new StatementValue(lines);
    }

    private ScopeValue VisitBlock(IScope scope, ScratchScriptParser.BlockContext context)
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
                case ScopeValue scopeValue:
                {
                    scope.Content.Add(scopeValue.Scope.ToString(Settings.CommandSeparator));
                    break;
                }
                case StatementValue statementValue:
                {
                    scope.Content.AddRange(statementValue.Dependencies ?? []);
                    scope.Content.AddRange(statementValue.Commands);
                    scope.Content.AddRange(statementValue.Cleanup ?? []);
                    break;
                }
                default:
                {
                    scope.Content.Add(value.ToString());
                    break;
                }
            }
        }

        _scope = scope.ParentScope;
        return new ScopeValue(scope);
    }

    public override TypedValue? VisitLine(ScratchScriptParser.LineContext context)
    {
        if (_scope is Scratch3Scope scope) scope.IntermediateStackCount = 0;

        if (context.statement() != null) return VisitStatement(context.statement());
        return base.VisitLine(context);
    }

    private IScope CreateDefaultScope()
    {
        return Target switch
        {
            CompilerTarget.Scratch3 => new Scratch3Scope(),
            _ => throw new NotImplementedException()
        };
    }

    private IFunctionScope CreateFunctionScope()
    {
        return Target switch
        {
            CompilerTarget.Scratch3 => new Scratch3FunctionScope(),
            _ => throw new NotImplementedException()
        };
    }
}
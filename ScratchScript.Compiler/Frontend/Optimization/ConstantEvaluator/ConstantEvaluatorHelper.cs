using System.Text;
using Antlr4.Runtime;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.GeneratedVisitor;
using ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator.Implementation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Optimization.ConstantEvaluator;

public static class ConstantEvaluatorHelper
{
    public static bool IsConstant(TypedValue? value)
    {
        return value switch
        {
            null => false,
            IdentifierExpressionValue
            {
                IdentifierType: IdentifierType.Variable or IdentifierType.FunctionArgument, RelatedScope: not null
            } identifierExpression => IsConstant(identifierExpression.RelatedScope
                .GetVariable(identifierExpression.Identifier)
                ?.LastKnownValue),
            ExpressionValue { ContainsIntermediateRepresentation: true } or StatementValue or ScopeValue => false,
            _ => true
        };
    }

    public static TypedValue Evaluate(string expression, Dictionary<string, TypedValue> parameters)
    {
        var sb = new StringBuilder();
        foreach (var (name, value) in parameters)
            sb.AppendLine($"{name} = {ConvertTypedValue(value)}");
        sb.Append(expression);

        var source = sb.ToString();
        Console.WriteLine($"=== executing ===\n{source}");
        var inputStream = new AntlrInputStream(source);
        var lexer = new ScratchCELexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ScratchCEParser(tokenStream);
        var visitor = new ScratchCEVisitor();

        var result = visitor.VisitProgram(parser.program());
        if (result == null) throw new Exception($"The evaluator returned null. (expression was \"{expression}\")");
        Console.WriteLine($"result = {result}\n");
        return result;
    }

    public static string ConvertTypedValue(TypedValue value)
    {
        if (value.Value == null) throw new Exception("The value was null.");

        if (value is IdentifierExpressionValue { RelatedScope: not null } identifierExpression &&
            identifierExpression.RelatedScope.GetVariable(identifierExpression.Identifier) is
                { LastKnownValue: not null } variable)
            return ConvertTypedValue(variable.LastKnownValue);

        if (value.Type == ScratchType.Boolean) return value.Value.ToString()!.ToLowerInvariant();
        if (value.Type == ScratchType.String) return value.Value.ToString()!.Surround('"');
        return value.Value!.ToString() ?? "";
    }
}
using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using ScratchScript.Compiler.AST.Representation;
using ScratchType = ScratchScript.Compiler.TypeChecker.ScratchType;

namespace ScratchScript.Compiler.Extensions;

public static class IrNodeExtensions
{
    [return: NotNullIfNotNull(nameof(node))]
    public static T? WithContext<T>(this T? node, ParserRuleContext context) where T : IrNode =>
        node == null ? null : node with { Context = context };

    public static IrExpressionNode WithInferredType(this IrExpressionNode node, ScratchType type) =>
        node with { InferredType = type };
}
using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchType = ScratchScript.Compiler.TypeChecker.ScratchType;

namespace ScratchScript.Compiler.Extensions;

public static class IrNodeExtensions
{
    extension<T>(T? node) where T : IrNode
    {
        [return: NotNullIfNotNull(nameof(node))]
        public T? WithContext(ParserRuleContext context) =>
            node == null ? null : node with { Context = context };

        [return: NotNullIfNotNull(nameof(node))]
        public T? OverrideIfNoContext(ParserRuleContext context) =>
            node == null ? null : node.Context == ParserRuleContext.EMPTY ? node with { Context = context } : node;

        [return: NotNullIfNotNull(nameof(node))]
        public T? WithFlag(string flag)
            => node == null ? null : node with { Flags = node.Flags.Add(flag) };
    }

    extension(IrExpressionNode node)
    {
        public IrExpressionNode WithInferredType(ScratchType type) =>
            node with { InferredType = type };

        public IrComplexExpressionNode ToComplex() =>
            node as IrComplexExpressionNode ?? new IrComplexExpressionNode(node);

        public IrExpressionNode Simplify() => node is IrComplexExpressionNode complex
            ? complex.Dependencies == null && complex.Cleanup == null ? complex.Expression.Simplify() : complex
            : node;
    }

    extension(IrCommandNode node)
    {
        public IrCommandSequenceNode ToSequence() => node as IrCommandSequenceNode ?? new IrCommandSequenceNode([node]);
    }
}
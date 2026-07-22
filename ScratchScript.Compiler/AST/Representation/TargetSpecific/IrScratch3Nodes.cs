using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.AST.Representation.TargetSpecific;

public record IrScratch3SynthesizedWhileLoopNode(
    IrComplexExpressionNode Condition,
    IrBlockNode Body) : IrCommandNode, ITargetSpecificNode
{
    public bool HasBreak => (Body.Scope as LoopScope ?? throw new Exception()).HasBreak;
    public bool HasContinue => (Body.Scope as LoopScope ?? throw new Exception()).HasContinue;

    public int GetNodeHash() => HashCode.Combine(IrHasher.GetNodeHash(Condition),
        IrHasher.GetNodeHash(Body));

    public IrNode Rewrite(Func<IrNode, IrNode> visit) => this with
    {
        Body = (IrBlockNode)visit(Body),
        Condition = ((IrExpressionNode)visit(Condition)).ToComplex()
    };
}

public interface IScratch3ExtensionRewriter
{
    public IrNode VisitScratch3SpecificNode(ITargetSpecificNode node, Func<IrNode, IrNode> visitor) => node switch
    {
        IrScratch3SynthesizedWhileLoopNode wh => VisitSynthesizedWhileLoop(wh, visitor),
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };

    public IrNode VisitSynthesizedWhileLoop(IrScratch3SynthesizedWhileLoopNode node, Func<IrNode, IrNode> visitor) =>
        node.Rewrite(visitor);
}
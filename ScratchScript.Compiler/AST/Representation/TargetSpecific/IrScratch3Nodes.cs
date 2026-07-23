using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.AST.Representation.TargetSpecific;

public interface IScratch3ExtensionRewriter
{
    public IrNode VisitScratch3SpecificNode(ITargetSpecificNode node, Func<IrNode, IrNode> visitor) => node switch
    {
        _ => throw new ArgumentOutOfRangeException(nameof(node))
    };
}
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.AST.Representation.TargetSpecific;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.Rewriters.Optimizations.HighLevel;

// TODO: apparently this might not even be needed???
public class LoopSynthesisRewriter : IrRewriter
{
    public const string SyntheticWhileFlag = "SYNTHETIC_WHILE";

    public override IrNode VisitWhileCommand(IrWhileCommandNode node)
    {
        /*
         *  while loop structure:
         *
         *  {condition dependencies}
         *  while(condition) {
         *       {body}
         *       {condition cleanup}
         *       {condition dependencies} <- because it needs to be recalculated
         *  }
         *  {condition cleanup} <- if the loop exits, the leftover data is still there,
         *                         so it needs to be cleaned
         */
        if (node.Flags.Contains(SyntheticWhileFlag)) return node;

        // ideal case where we don't need to do anything
        if (Visit(node.Condition) is not IrComplexExpressionNode &&
            node.Body.Scope is LoopScope { HasBreak: false, HasContinue: false } loopScope)
        {
            if (loopScope.NextIterationPrerequisite != null) loopScope.Body.Add(loopScope.NextIterationPrerequisite);
            return node.WithFlag(SyntheticWhileFlag);
        }

        var condition = ((IrExpressionNode)Visit(node.Condition)).ToComplex();
        return new IrScratch3SynthesizedWhileLoopNode(condition, node.Body);
    }
}
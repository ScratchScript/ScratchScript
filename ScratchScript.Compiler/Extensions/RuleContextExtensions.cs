using Antlr4.Runtime;

namespace ScratchScript.Compiler.Extensions;

public static class RuleContextExtensions
{
    public static T? GetParent<T>(this RuleContext context) where T : RuleContext
    {
        while (true)
        {
            if (context is T ruleContext) return ruleContext;
            if (context.Parent == null) return null;
            context = context.Parent;
        }
    }
}
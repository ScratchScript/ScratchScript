using Antlr4.Runtime;

namespace ScratchScript.Extensions;

public static class RuleContextExtensions
{
	public static T FirstParentOfType<T>(this RuleContext context) where T : RuleContext
	{
		return (context as T ?? (context.Parent != null ? FirstParentOfType<T>(context.Parent) : null)) ??
		       throw new InvalidOperationException();
	}
}
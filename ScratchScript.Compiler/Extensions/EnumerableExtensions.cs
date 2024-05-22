namespace ScratchScript.Compiler.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T>? ConcatNullable<T>(this IEnumerable<T>? to, IEnumerable<T>? what)
    {
        return to?.Concat(what ?? []);
    }
}
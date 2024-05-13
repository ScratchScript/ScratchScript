namespace ScratchScript.Compiler.Extensions;

public static class StringExtensions
{
    public static string ToAnsiConsoleCompatible(this string str)
    {
        return str.Replace("[", "[[").Replace("]", "]]");
    }
}
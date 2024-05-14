namespace ScratchScript.Compiler.Extensions;

public static class StringExtensions
{
    public static string ToAnsiConsoleCompatible(this string str)
    {
        return str.Replace("[", "[[").Replace("]", "]]");
    }
    
    public static string Surround(this string str, char with) => with + str + with;
}
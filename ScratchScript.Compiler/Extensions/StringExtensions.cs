namespace ScratchScript.Compiler.Extensions;

public static class StringExtensions
{
    public static string ToAnsiConsoleCompatible(this string str)
    {
        return str.Replace("[", "[[").Replace("]", "]]");
    }

    public static string Surround(this string str, char with)
    {
        return with + str + with;
    }

    public static string Combine(this string str, char separator, params string[] with)
    {
        return string.Join(separator, new List<string>([str, ..with]).Where(s => !string.IsNullOrEmpty(s)));
    }
}
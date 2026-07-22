using ScratchType = ScratchScript.Compiler.TypeChecker.ScratchType;

namespace ScratchScript.Compiler.Extensions;

public static class StringExtensions
{
    public static string GetFunctionSignatureString(string name, IEnumerable<ScratchType> types)
    {
        return $"{name}({string.Join(", ", types.Select(type => type.ToString()))})";
    }

    extension(string str)
    {
        public string ToAnsiConsoleCompatible() => str.Replace("[", "[[").Replace("]", "]]");

        public string Surround(char with) => with + str + with;

        public string Combine(char separator, params string[] with)
        {
            return string.Join(separator, new List<string>([str, ..with]).Where(s => !string.IsNullOrEmpty(s)));
        }

        public string Capitalize() => string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1));
    }
}
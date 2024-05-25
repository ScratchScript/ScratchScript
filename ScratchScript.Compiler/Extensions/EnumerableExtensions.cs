using System.Security.Cryptography;

namespace ScratchScript.Compiler.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T>? ConcatNullable<T>(this IEnumerable<T>? to, IEnumerable<T>? what)
    {
        return to?.Concat(what ?? []);
    }

    public static string ToMd5Checksum(this IEnumerable<byte> array)
    {
        return BitConverter.ToString(MD5.HashData(array.ToArray())).Replace("-", "").ToLowerInvariant();
    }
}
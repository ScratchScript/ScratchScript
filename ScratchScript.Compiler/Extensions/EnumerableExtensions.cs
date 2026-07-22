using System.Security.Cryptography;

namespace ScratchScript.Compiler.Extensions;

public static class EnumerableExtensions
{
    public static string ToMd5Checksum(this IEnumerable<byte> array) =>
        Convert.ToHexStringLower(MD5.HashData(array.ToArray()));

    extension<T>(IEnumerable<T>? to)
    {
        public IEnumerable<T> ConcatNullable(IEnumerable<T>? what) => (to ?? []).Concat(what ?? []);

        public IEnumerable<T> ConcatNullable(T? what) => (to ?? []).Concat(what != null ? [what] : []);
    }
}
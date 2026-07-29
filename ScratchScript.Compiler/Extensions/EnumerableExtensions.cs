using System.Security.Cryptography;
using System.Text;

namespace ScratchScript.Compiler.Extensions;

public static class EnumerableExtensions
{
    public static string ToMd5Checksum(IEnumerable<byte> array) =>
        Convert.ToHexStringLower(MD5.HashData(array.ToArray()));

    public static string ToMd5Checksum(string str) =>
        ToMd5Checksum(Encoding.UTF8.GetBytes(str));


    extension<T>(IReadOnlyList<T>? to)
    {
        public IReadOnlyList<T> ConcatNullable(IEnumerable<T>? what) => (to ?? []).Concat(what ?? []).ToList();

        public IReadOnlyList<T> ConcatNullable(T? what) => (to ?? []).Concat(what != null ? [what] : []).ToList();
    }
}
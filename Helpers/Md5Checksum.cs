using System.Security.Cryptography;

namespace ScratchScript.Helpers;

public static class Md5Checksum
{
	public static string FromFile(string file)
	{
		using var stream = File.OpenRead(file);
		return FromStream(stream);
	}

	public static string FromStream(Stream stream)
	{
		return BitConverter.ToString(MD5.Create().ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
	}
}
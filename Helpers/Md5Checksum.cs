using System.Security.Cryptography;

namespace ScratchScript.Helpers;

public class Md5Checksum
{
	public static string FromFile(string file)
	{
		using var md5 = MD5.Create();
		using var stream = File.OpenRead(file);
		return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
	}
}
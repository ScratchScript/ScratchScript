using ScratchScript.Wrapper;

namespace ScratchScript.Extensions;

public static class BlockExtensions
{
	public static Block WithId(this Block block, string id)
	{
		block.Id = id;
		return block;
	}

	public static Block WithPurposeId(this Block block, string purpose)
	{
		block.Id = RandomId(purpose);
		return block;
	}

	public static string RandomId(string purpose) => $"ScratchScript_{purpose}_{Guid.NewGuid():N}";
}
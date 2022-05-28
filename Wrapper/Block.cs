namespace ScratchScript.Wrapper;

public class Block
{
	public string opcode;
	public string? next;
	public string? parent;
	public Dictionary<string, List<object>> inputs = new();
	public Dictionary<string, List<object>> fields = new();
	public bool shadow;
	public bool topLevel => parent == null;
	public int? x;
	public int? y;

	[NonSerialized] public string Id;
	[NonSerialized] public Dictionary<string, object> AdditionalData;
}
namespace ScratchScript.Wrapper;

public class Mutation
{
	public string tagName = "mutation";
	public object[] children = Array.Empty<object>();
	public string proccode;
	public List<string> argumentids = new();
	public List<string> argumentnames = new();
	public List<object> argumentdefaults = new();
	public bool warp;
}
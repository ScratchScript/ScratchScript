namespace ScratchScript.Wrapper;

public class Mutation
{
	public List<object> argumentdefaults = new();
	public List<string> argumentids = new();
	public List<string> argumentnames = new();
	public object[] children = Array.Empty<object>();
	public string proccode;
	public string tagName = "mutation";
	public bool warp;
}
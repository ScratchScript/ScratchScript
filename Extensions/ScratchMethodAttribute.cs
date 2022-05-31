namespace ScratchScript.Extensions;

[AttributeUsage(AttributeTargets.Method)]
public class ScratchMethodAttribute : Attribute
{
	public ScratchMethodAttribute(string name)
	{
		Name = name;
	}

	public string Name { get; }
}
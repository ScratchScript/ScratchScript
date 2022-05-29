namespace ScratchScript.Extensions;

[AttributeUsage(AttributeTargets.Method)]
public class ScratchMethodAttribute: Attribute
{ 
	public string Name { get; }

	public ScratchMethodAttribute(string name)
	{
		Name = name;
	}
}
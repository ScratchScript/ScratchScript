namespace ScratchScript.Extensions;

[AttributeUsage(AttributeTargets.Method)]
public class NotForStageAttribute: Attribute
{
	public NotForStageAttribute()
	{
	}
}
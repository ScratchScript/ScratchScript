using ScratchScript.Types;

namespace ScratchScript.Helpers;

public class TypeHelper
{
	private static Dictionary<Type, int> _scratchIds = new()
	{
		{typeof(float), 4},
		{typeof(uint), 6},
		{typeof(int), 7},
		{typeof(ScratchAngle), 8},
		{typeof(ScratchColor), 9},
		{typeof(string), 10}
	};
	//broadcasts and others are not variable types.
	
	public static int ScratchIdFromValue(object obj) => _scratchIds[obj.GetType()];
}
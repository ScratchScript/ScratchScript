using ScratchScript.Helpers;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Compiler;

public class TargetCompiler
{
	public Target WrappedTarget = new();
	public Dictionary<string, string> Variables = new();

	public string Name
	{
		get => WrappedTarget.name;
		set => WrappedTarget.name = value;
	}

	public int LayerOrder
	{
		get => WrappedTarget.layerOrder;
		set => WrappedTarget.layerOrder = value;
	}

	public void CreateVariable(string name, object value)
	{
		var id = "ScratchScript_Variable_" + Guid.NewGuid().ToString("N");
		Variables[name] = id;

		WrappedTarget.variables[id] = new List<object>
		{
			name,
			value
		};
	}

	public TargetCompiler()
	{
		WrappedTarget = new Target
		{
			isStage = ProjectCompiler.Current.TargetCompilerCount == 0,
			currentCostume = 0,
			volume = 100,
			tempo = 60,
			videoTransparency = 50,
			videoState = "on",
			textToSpeechLanguage = null,
			x = 0,
			y = 0,
			size = 100,
			direction = 90,
			draggable = false,
			rotationStyle = "all around"
		};
		LayerOrder = ProjectCompiler.Current.TargetCompilerCount;
	}
}
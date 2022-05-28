using ScratchScript.Extensions;
using ScratchScript.Blocks;
using ScratchScript.Helpers;
using ScratchScript.Types;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Compiler;

public class TargetCompiler
{
	public Target WrappedTarget = new();
	public Dictionary<string, ScratchVariable> Variables = new();

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

	public void ReplaceBlock(Block newBlock) => WrappedTarget.blocks[newBlock.Id] = newBlock;

	public void CreateVariable(string name, object value)
	{
		var id = "ScratchScript_Variable_" + Guid.NewGuid().ToString("N");
		Variables[name] = new ScratchVariable
		{
			Id = id,
			Name = name
		};

		WrappedTarget.variables[id] = new List<object>
		{
			name,
			value
		};
	}
	public Block CreateBlock(Block block, bool ignoreNext = false, bool ignoreParent = false)
	{
		if (string.IsNullOrEmpty(block.Id))
			block = block.WithPurposeId("Unknown");

		if(WrappedTarget.blocks.Count != 0 && !ignoreNext)
			WrappedTarget.blocks.Last().Value.next = block.Id;
		if(!ignoreParent)
			block.parent = WrappedTarget.blocks.Count == 0 ? null: WrappedTarget.blocks.Last().Key;
		if(WrappedTarget.blocks.Count != 0)
			Log.Information($"{block.parent ?? "none"} {WrappedTarget.blocks.Last().Value.next ?? "none"}");
		WrappedTarget.blocks[block.Id] = block;
		return block;
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
		CreateBlock(Event.WhenFlagClicked().WithPurposeId("EntryPoint"));
	}
}
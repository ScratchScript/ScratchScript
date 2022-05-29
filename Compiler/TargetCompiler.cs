using System.Runtime.InteropServices;
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
			Name = name,
			Type = value.GetType()
		};

		WrappedTarget.variables[id] = new List<object>
		{
			name,
			value
		};
	}

	public string? PendingComment;
	public Block CreateBlock(Block block, bool ignoreNext = false, bool ignoreParent = false)
	{

		if (string.IsNullOrEmpty(block.Id))
			block = block.WithPurposeId("Unknown");

		switch (string.IsNullOrEmpty(PendingComment))
		{
			case false when string.IsNullOrEmpty(block.comment) && !block.shadow:
				block.comment = PendingComment;
				WrappedTarget.comments[PendingComment!].blockId = block.Id;
				Log.Debug("Attached comment {CommentId} to block {BlockId}", PendingComment, block.Id);
				PendingComment = null;
				break;
			case false when !string.IsNullOrEmpty(block.comment) || block.shadow:
				Log.Warning("Comment was not attachable to a block");
				break;
		}

		if(WrappedTarget.blocks.Count != 0 && !ignoreNext)
			WrappedTarget.blocks.Last(x => !x.Value.shadow).Value.next = block.Id;
		if(!ignoreParent)
			block.parent = WrappedTarget.blocks.Count == 0 ? null: WrappedTarget.blocks.Last(x => !x.Value.shadow).Key;
		WrappedTarget.blocks[block.Id] = block;
		return block;
	}

	/*private void SetNextPosition(ref Block block)
	{
		if (WrappedTarget.blocks.Count == 0)
		{
			block.x = 0;
			block.y = 0;
		}
		else
		{
			if (block.topLevel && !block.shadow)
			{
				var last = WrappedTarget.blocks.Last(x => x.Value.topLevel).Value;
				if (last.x + 500 > 3000)
				{
					block.x = 0;
					block.y = last.y + 1000;
				}
				else block.x = last.x + 500;
			}
			else if(!block.topLevel && !block.shadow)
			{
				var last = WrappedTarget.blocks.Last(x => !x.Value.topLevel && !x.Value.shadow).Value;
				block.y = last.y + 50;
			}
		}
	}*/

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
using System.ComponentModel.DataAnnotations.Schema;

namespace ScratchScript.Wrapper;

public class Target
{
	public bool isStage;
	public string name;
	public Dictionary<string, List<object>> variables = new();
	public Dictionary<string, List<object>> lists = new();
	public Dictionary<string, string> broadcasts = new();
	public Dictionary<string, Block> blocks = new();
	public Dictionary<string, Comment> comments = new();
	public int currentCostume;
	public List<Asset> costumes = new();
	public List<Asset> sounds = new();
	public int layerOrder;
	public float volume;

	public int? tempo;
	public string? videoState;
	public int? videoTransparency;
	public string? textToSpeechLanguage;

	public bool? visible;
	public int? x;
	public int? y;
	public float? size;
	public float? direction;
	public bool? draggable;
	public string? rotationStyle;
}
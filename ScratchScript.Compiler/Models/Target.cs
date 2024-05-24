namespace ScratchScript.Compiler.Models;

public record Target
{
    public Dictionary<string, Block> Blocks = [];
    public Dictionary<string, string> Broadcasts = [];
    public Dictionary<string, Comment> Comments = [];
    public List<Costume> Costumes = [];
    public int CurrentCostume;
    public bool IsStage;
    public int LayerOrder;
    public Dictionary<string, List<object>> Lists = [];
    public string Name;
    public List<Sound> Sounds = [];
    public Dictionary<string, List<object>> Variables = [];
    public int Volume = 100;
}
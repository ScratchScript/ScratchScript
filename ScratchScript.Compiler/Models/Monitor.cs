using Newtonsoft.Json;

namespace ScratchScript.Compiler.Models;

public record Monitor
{
    public float Height;
    public string Id;
    public bool IsDiscrete;
    public string Mode;
    public string Opcode;
    [JsonProperty("params")] public Dictionary<string, string> Parameters = [];
    public float SliderMax;
    public float SliderMin;
    public string SpriteName;
    public object Value;
    public bool Visible;
    public float Width;
    public float X;
    public float Y;
}
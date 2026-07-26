using System.Text.Json.Serialization;

namespace ScratchScript.Compiler.ProjectEmitter.Models;

public record Block
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Comment;

    public Dictionary<string, List<object>> Fields = [];

    [JsonIgnore] public string Id;
    public Dictionary<string, List<object>> Inputs = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Mutation? Mutation;

    public string Next;
    public string Opcode;
    public string Parent;
    public bool Shadow;
    public bool TopLevel;
    public float X;
    public float Y;
}
using System.Text.Json.Serialization;

namespace ScratchScript.Compiler.ProjectEmitter.Models;

public record Mutation
{
    [JsonPropertyName("argumentdefaults")] public string ArgumentDefaults = "[]";
    [JsonPropertyName("argumentids")] public string ArgumentIds = "[]";
    [JsonPropertyName("argumentnames")] public string ArgumentNames = "[]";
    public List<object> Children = [];
    [JsonPropertyName("hasnext")] public bool HasNext;
    [JsonPropertyName("proccode")] public string ProcedureCode;
    public string TagName = "mutation";
    public bool Warp;
}
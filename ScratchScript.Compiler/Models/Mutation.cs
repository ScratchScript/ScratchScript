using Newtonsoft.Json;

namespace ScratchScript.Compiler.Models;

public record Mutation
{
    [JsonProperty("argumentdefaults")] public string ArgumentDefaults = "[]";
    [JsonProperty("argumentids")] public string ArgumentIds = "[]";
    [JsonProperty("argumentnames")] public string ArgumentNames = "[]";
    public List<object> Children = [];
    [JsonProperty("hasnext")] public bool HasNext;
    [JsonProperty("proccode")] public string ProcedureCode;
    public string TagName = "mutation";
    public bool Warp;
}
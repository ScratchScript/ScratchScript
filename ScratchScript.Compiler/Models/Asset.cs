using Newtonsoft.Json;

namespace ScratchScript.Compiler.Models;

public record Asset
{
    public string AssetId;
    [NonSerialized] public byte[] Data;
    public string DataFormat;
    [JsonProperty("md5ext")] public string Md5Extension;
    public string Name;
}
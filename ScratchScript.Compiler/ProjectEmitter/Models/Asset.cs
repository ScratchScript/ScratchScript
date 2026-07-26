using System.Text.Json.Serialization;

namespace ScratchScript.Compiler.ProjectEmitter.Models;

public record Asset
{
    public string AssetId;
    [NonSerialized] public byte[] Data;
    public string DataFormat;
    [JsonPropertyName("md5ext")] public string Md5Extension;
    public string Name;
}
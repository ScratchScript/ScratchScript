using System.Text.Json.Serialization;

namespace ScratchScript.Compiler.ProjectEmitter.Models;

public class Metadata
{
    [JsonPropertyName("semver")] public string ScratchVersion = "3.0.0";
    [JsonPropertyName("agent")] public string UserAgent = "";
    [JsonPropertyName("vm")] public string VmVersion = "0.2.0";
}
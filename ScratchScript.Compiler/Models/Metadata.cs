using Newtonsoft.Json;

namespace ScratchScript.Compiler.Models;

public class Metadata
{
    [JsonProperty("semver")] public string ScratchVersion = "3.0.0";
    [JsonProperty("agent")] public string UserAgent = "";
    [JsonProperty("vm")] public string VmVersion = "0.2.0";
}
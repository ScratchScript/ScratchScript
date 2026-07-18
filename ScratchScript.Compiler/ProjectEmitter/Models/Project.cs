using Newtonsoft.Json;

namespace ScratchScript.Compiler.ProjectEmitter.Models;

public record Project
{
    public List<string> Extensions = [];

    [JsonProperty("meta")] public Metadata Metadata = new();
    public List<Monitor> Monitors = [];
    public List<Target> Targets = [];
}
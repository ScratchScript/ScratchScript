using System.Text.Json.Serialization;

namespace ScratchScript.Compiler.ProjectEmitter.Models;

public record Project
{
    public List<string> Extensions = [];

    [JsonPropertyName("meta")] public Metadata Metadata = new();
    public List<Monitor> Monitors = [];
    public List<Target> Targets = [];
}
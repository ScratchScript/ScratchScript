using System.Text.Json.Serialization;

namespace ScratchScript.Cli.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProjectType
{
    [JsonStringEnumMemberName("application")]
    Application,
    [JsonStringEnumMemberName("library")] Library
}

public record ProjectConfiguration(string Name, ProjectType Type);
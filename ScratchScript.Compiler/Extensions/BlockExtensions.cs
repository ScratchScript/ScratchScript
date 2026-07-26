using System.Text.Json;
using ScratchScript.Compiler.ProjectEmitter.Models;

namespace ScratchScript.Compiler.Extensions;

public static class BlockExtensions
{
    private static readonly JsonSerializerOptions _options = new()
        { IncludeFields = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };

    public static Block Clone(this Block original) =>
        JsonSerializer.Deserialize<Block>(JsonSerializer.Serialize(original, _options), _options) ??
        throw new Exception("Failed to deep clone a block.");
}
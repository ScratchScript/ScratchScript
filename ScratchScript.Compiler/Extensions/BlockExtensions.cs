using Newtonsoft.Json;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Extensions;

public static class BlockExtensions
{
    public static Block Clone(this Block original)
    {
        return JsonConvert.DeserializeObject<Block>(JsonConvert.SerializeObject(original)) ??
               throw new Exception("Failed to deep clone a block.");
    }
}
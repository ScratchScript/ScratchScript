using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Information;

public record ExportsStorage
{
    public readonly Dictionary<string, EnumScratchType> Enums = [];
    public readonly Dictionary<string, Scope> Events = [];
}
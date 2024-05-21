using ScratchScript.Compiler.Frontend.Targets;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Information;

public record ExportsStorage
{
    public readonly Dictionary<string, EnumScratchType> Enums = [];
    public readonly Dictionary<string, IScope> Events = [];
    public readonly Dictionary<string, IFunctionScope> Functions = [];
}
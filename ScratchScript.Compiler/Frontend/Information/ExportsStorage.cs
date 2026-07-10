using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Information;

public record ExportsStorage
{
    /*public readonly Dictionary<string, EnumScratchType> Enums = [];*/
    public readonly Dictionary<string, IrEventNode> Events = [];
    public readonly Dictionary<string, IrFunctionNode> Functions = [];
}
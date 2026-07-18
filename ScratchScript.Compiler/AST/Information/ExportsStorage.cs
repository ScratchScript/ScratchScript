using ScratchScript.Compiler.AST.Representation;

namespace ScratchScript.Compiler.AST.Information;

public record ExportsStorage
{
    /*public readonly Dictionary<string, EnumScratchType> Enums = [];*/
    public readonly Dictionary<string, IrEventNode> Events = [];
    public readonly Dictionary<string, IrFunctionNode> Functions = [];
}
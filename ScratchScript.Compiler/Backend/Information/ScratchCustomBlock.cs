using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.Implementation;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Information;

// TODO: if ANY of the targets start supporting first-class argument reporters, this should be modified to allow native scratch arguments to be added.
// for now, as it's not possible in any of the scratch mods (except snap!), there's no need for that functionality.
public class ScratchCustomBlock
{
    public Block Definition { get; }
    public Block Prototype { get; }
    public Block Call { get; }
    public Mutation Mutation { get; }
    
    public ScratchCustomBlock(string name, bool warp, Func<string, string> idGenerator)
    {
        Mutation = new Mutation
        {
            ProcedureCode = name,
            ArgumentIds = "[]",
            ArgumentNames = "[]",
            ArgumentDefaults = "[]",
            Warp = warp
        };
        
        Definition = new Block
        {
            Opcode = Function.Definition,
            Id = idGenerator(Function.Definition),
            TopLevel = true
        };
        Prototype = new Block
        {
            Opcode = Function.Prototype,
            Id = idGenerator(Function.Prototype),
            Shadow = true
        };
        Call = new Block { Opcode = Function.Call };

        Definition.Inputs["custom_block"] = [(int)ScratchShadowType.Shadow, Prototype.Id];
        Prototype.Parent = Definition.Id;
        
        Prototype.Mutation = Mutation;
        Call.Mutation = Mutation;
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Backend.Implementation;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Information;

public class ScratchCustomBlock
{
    public const string StackPointerName = "__sp";
    public const string IntermediateStackPointerName = "__isp";

    public ScratchCustomBlock(string name, bool warp, Func<string, string> idGenerator)
    {
        Name = name;
        Warp = warp;
        IdGenerator = idGenerator;

        Definition = new Block
        {
            Opcode = Function.Definition,
            Id = IdGenerator(Function.Definition),
            TopLevel = true
        };
        Prototype = new Block
        {
            Opcode = Function.Prototype,
            Id = IdGenerator(Function.Prototype),
            Shadow = true
        };
        Call = new Block { Opcode = Function.Call };

        Definition.Inputs["custom_block"] = [(int)ScratchShadowType.Shadow, Prototype.Id];
        Prototype.Parent = Definition.Id;

        RebuildMutation();
    }

    public string Name { get; init; }
    public bool Warp { get; init; }
    public Block Definition { get; }
    public Block Prototype { get; }
    public Block Call { get; }
    public Dictionary<string, Block> Reporters { get; } = [];
    public Mutation Mutation { get; private set; }

    private Func<string, string> IdGenerator { get; }

    public void AddReporter(string name)
    {
        Reporters[name] = new Block
        {
            Opcode = Function.ReporterStringNumber,
            Id = IdGenerator(Function.ReporterStringNumber),
            Shadow = true,
            Parent = Prototype.Id,
            Fields =
            {
                ["VALUE"] = [name, 0]
            }
        };
        RebuildMutation();
    }

    private void RebuildMutation()
    {
        var procedureCode = string.Join(' ', [Name, ..Enumerable.Repeat("%s", Reporters.Count)]);
        var ids = new JArray();
        var names = new JArray();
        var defaults = new JArray();

        foreach (var (name, block) in Reporters)
        {
            ids.Add(block.Id);
            names.Add(name);
            defaults.Add("");
        }

        Mutation = new Mutation
        {
            ProcedureCode = procedureCode,
            ArgumentIds = ids.ToString(Formatting.None),
            ArgumentNames = names.ToString(Formatting.None),
            ArgumentDefaults = defaults.ToString(Formatting.None),
            Warp = Warp
        };

        Prototype.Mutation = Mutation;
        Call.Mutation = Mutation;
    }
}
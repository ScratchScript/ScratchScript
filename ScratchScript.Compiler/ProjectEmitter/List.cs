using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.ProjectEmitter.Blocks;
using ScratchScript.Compiler.ProjectEmitter.Models;

namespace ScratchScript.Compiler.ProjectEmitter;

public partial class ScratchScriptProjectEmitter
{
    public override object? VisitPushCommand(IrPushCommand node)
    {
        var value = Visit(node.Expression);
        if (value == null) throw new Exception("The 'value' expression in VisitPushCommand was null.");

        var block = new Block { Opcode = Data.AddToList, Id = GenerateBlockId(Data.AddToList) };
        block.Fields["LIST"] = CreateField(node.List);
        block.Inputs["ITEM"] = value is Block valueBlock ? CreateInput(valueBlock, block) : CreateInput(value);
        return block;
    }

    public override object? VisitPushAtCommand(IrPushAtCommand node)
    {
        var index = Visit(node.Where);
        var value = Visit(node.Expression);
        if (index == null) throw new Exception("The 'index' expression in VisitPushAtCommand was null.");
        if (value == null) throw new Exception("The 'value' expression in VisitPushAtCommand was null.");

        var block = new Block { Opcode = Data.InsertIntoList, Id = GenerateBlockId(Data.InsertIntoList) };
        block.Fields["LIST"] = CreateField(node.List);
        block.Inputs["INDEX"] = index is Block indexBlock ? CreateInput(indexBlock, block) : CreateInput(index);
        block.Inputs["ITEM"] = value is Block valueBlock ? CreateInput(valueBlock, block) : CreateInput(value);
        return block;
    }

    public override object? VisitPopCommand(IrPopCommand node)
    {
        var block = new Block { Opcode = Data.DeleteFromList, Id = GenerateBlockId(Data.DeleteFromList) };
        block.Fields["LIST"] = CreateField(node.List);
        block.Inputs["INDEX"] = CreateInput(1);
        return block;
    }

    public override object? VisitPopAtCommand(IrPopAtCommand node)
    {
        var index = Visit(node.Where);
        if (index == null) throw new Exception("The 'index' expression in VisitPopAtCommand was null.");

        var block = new Block { Opcode = Data.DeleteFromList, Id = GenerateBlockId(Data.DeleteFromList) };
        block.Fields["LIST"] = CreateField(node.List);
        block.Inputs["INDEX"] = index is Block indexBlock ? CreateInput(indexBlock, block) : CreateInput(index);
        return block;
    }

    public override object? VisitPopAllCommand(IrPopAllCommand node)
    {
        return new Block
        {
            Opcode = Data.DeleteAllOfList, Id = GenerateBlockId(Data.DeleteAllOfList),
            Fields =
            {
                ["LIST"] = CreateField(node.List)
            }
        };
    }
}
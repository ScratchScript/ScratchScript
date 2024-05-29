using ScratchScript.Compiler.Backend.Blocks;
using ScratchScript.Compiler.Models;

namespace ScratchScript.Compiler.Backend.Implementation;

public partial class ScratchIRVisitor
{
    public override object VisitPushCommand(ScratchIRParser.PushCommandContext context)
    {
        var list = context.Identifier().GetText();
        var value = Visit(context.expression());
        if (value == null) throw new Exception("An expression in VisitPushCommand was null.");

        var block = new Block { Opcode = Data.AddToList, Id = GenerateBlockId(Data.AddToList) };
        block.Fields["LIST"] = CreateField(list);
        block.Inputs["ITEM"] = value is Block valueBlock ? CreateInput(valueBlock, block) : CreateInput(value);
        return block;
    }

    public override object VisitPushAtCommand(ScratchIRParser.PushAtCommandContext context)
    {
        var list = context.Identifier().GetText();
        var index = Visit(context.expression(0));
        var value = Visit(context.expression(1));
        if (index == null || value == null) throw new Exception("An expression in VisitPushCommand was null.");

        var block = new Block { Opcode = Data.InsertIntoList, Id = GenerateBlockId(Data.InsertIntoList) };
        block.Fields["LIST"] = CreateField(list);
        block.Inputs["INDEX"] = index is Block indexBlock ? CreateInput(indexBlock, block) : CreateInput(index);
        block.Inputs["ITEM"] = value is Block valueBlock ? CreateInput(valueBlock, block) : CreateInput(value);
        return block;
    }

    public override object VisitPopCommand(ScratchIRParser.PopCommandContext context)
    {
        var list = context.Identifier().GetText();
        var block = new Block { Opcode = Data.DeleteFromList, Id = GenerateBlockId(Data.DeleteFromList) };
        block.Fields["LIST"] = CreateField(list);
        block.Inputs["INDEX"] = CreateInput(1);
        return block;
    }

    public override object VisitPopAtCommand(ScratchIRParser.PopAtCommandContext context)
    {
        var list = context.Identifier().GetText();
        var index = Visit(context.expression());
        if (index == null) throw new Exception("An expression in VisitPopAtCommand was null.");

        var block = new Block { Opcode = Data.DeleteFromList, Id = GenerateBlockId(Data.DeleteFromList) };
        block.Fields["LIST"] = CreateField(list);
        block.Inputs["INDEX"] = index is Block indexBlock ? CreateInput(indexBlock, block) : CreateInput(index);
        return block;
    }

    public override object VisitPopAllCommand(ScratchIRParser.PopAllCommandContext context)
    {
        var list = context.Identifier().GetText();
        var block = new Block
        {
            Opcode = Data.DeleteAllOfList, Id = GenerateBlockId(Data.DeleteAllOfList),
            Fields =
            {
                ["LIST"] = CreateField(list)
            }
        };
        return block;
    }
}
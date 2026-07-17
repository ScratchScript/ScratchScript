using ScratchScript.Compiler.Backend.Information;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Backend.Rewriters.Optimizations.HighLevel;

public class RawFunctionsExpansionRewriter : IrRewriter
{
    private static (string Opcode, Dictionary<string, IrExpressionNode> Inputs, Dictionary<string, IrExpressionNode> Fields,
        ScratchType? ExpectedType)
        ParseArguments(Dictionary<string, IrExpressionNode> arguments)
    {
        if (arguments["opcode"] is not IrConstantExpressionNode rawOpcode ||
            rawOpcode.Value.Type != ScratchType.String)
            throw new Exception();
        var opcode = (string)rawOpcode.Value.Value!;

        ScratchType? type = null;
        if (arguments.TryGetValue("type", out var typeExpression))
        {
            if (typeExpression is not IrConstantExpressionNode rawType || rawType.Value.Type != ScratchType.String)
                throw new Exception();
            type = ScratchType.FromString((string)rawType.Value.Value!);
        }

        if (arguments["data"] is not IrConstantExpressionNode rawData || rawData.Value.Type != ScratchType.Object)
            throw new Exception();
        var data = (Dictionary<string, IrExpressionNode>)rawData.Value.Value!;
        if (data.TryGetValue("inputs", out var inputsExpression) &&
            (inputsExpression is not IrConstantExpressionNode rawInputs ||
             rawInputs.Value.Type != ScratchType.Object)) throw new Exception();
        if (data.TryGetValue("fields", out var fieldsExpression) &&
            (fieldsExpression is not IrConstantExpressionNode rawFields ||
             rawFields.Value.Type != ScratchType.Object)) throw new Exception();
        var inputs = inputsExpression != null
            ? (Dictionary<string, IrExpressionNode>)((IrConstantExpressionNode)inputsExpression).Value.Value!
            : [];
        var fields = fieldsExpression != null
            ? (Dictionary<string, IrExpressionNode>)((IrConstantExpressionNode)fieldsExpression).Value.Value!
            : [];

        return (opcode, inputs, fields, type);
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        if (node.Function != ReservedNames.RawStatementFunction) return node;
        var (opcode, inputs, fields, _) = ParseArguments(node.Arguments);
        return new IrRawCommandNode(opcode, inputs, fields);
    }

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        if (node.Function != ReservedNames.RawExpressionFunction) return node;
        var (opcode, inputs, fields, expectedType) = ParseArguments(node.Arguments);
        return new IrShadowExpressionNode(opcode, inputs, fields, expectedType);
    }
}
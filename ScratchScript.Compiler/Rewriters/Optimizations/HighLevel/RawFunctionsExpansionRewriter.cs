using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.Rewriters.Optimizations.HighLevel;

public class RawFunctionsExpansionRewriter : IrRewriter
{
    private static (string Opcode, Dictionary<string, IrExpressionNode> Inputs, Dictionary<string, IrExpressionNode>
        Fields,
        ScratchType? ExpectedType)
        ParseArguments(List<(string?, IrExpressionNode)> arguments)
    {
        IrExpressionNode? GetArgument(string name) => arguments.FirstOrDefault(a => a.Item1 == name).Item2;

        if (GetArgument("opcode") is not IrConstantExpressionNode rawOpcode ||
            rawOpcode.Value.Type != ScratchType.String)
            throw new Exception();
        var opcode = (string)rawOpcode.Value.Value!;

        ScratchType? type = null;
        if (GetArgument("type") != null)
        {
            if (GetArgument("type") is not IrConstantExpressionNode rawType || rawType.Value.Type != ScratchType.String)
                throw new Exception();
            type = ScratchType.FromString((string)rawType.Value.Value!);
        }

        if (GetArgument("data") is not IrConstantExpressionNode rawData || rawData.Value.Type != ScratchType.Object)
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
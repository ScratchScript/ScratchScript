using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Backend.Rewriters.TargetLowering;

static class Scratch3CommandHelper
{
    public static IrShadowExpressionNode IndexOf(string list, IrExpressionNode item) => IrShadowBuilder
        .FromOpcode("data_itemnumoflist")
        .WithField("LIST", list)
        .WithInput("ITEM", item)
        .BuildExpression(ScratchType.Number);

    public static IrShadowExpressionNode ItemAt(string list, IrExpressionNode index) => IrShadowBuilder
        .FromOpcode("data_itemoflist")
        .WithField("LIST", list)
        .WithInput("INDEX", index)
        .BuildExpression();

    public static IrShadowExpressionNode LengthOf(string list) => IrShadowBuilder
        .FromOpcode("data_lengthoflist")
        .WithField("LIST", list)
        .BuildExpression(ScratchType.Number);

    public static IrRawCommandNode Replace(string list, IrExpressionNode index, IrExpressionNode value) =>
        IrShadowBuilder
            .FromOpcode("data_replaceitemoflist")
            .WithField("LIST", list)
            .WithInput("INDEX", index)
            .WithInput("ITEM", value)
            .BuildCommand();

    public static IrRawCommandNode SetVariable(string id, IrExpressionNode value) =>
        Replace(Scratch3LoweringPass.VariableValuesList,
            IndexOf(Scratch3LoweringPass.VariableNamesList, new IrConstantExpressionNode(TypedValue.String(id))),
            value);
}

public class Scratch3LoweringPass : IrRewriter
{
    public const string StackList = "__Stack";
    public const string VariableNamesList = "__VN";
    public const string VariableValuesList = "__VV";
    public const string StackPointerReporter = "arg:__sp";

    private Scope? _scope;
    private HashSet<string> _addedVariables = [];

    private string GenerateVariableId(string name)
        => $"{_scope?.Depth ?? -1}_{name}";

    public override IrNode VisitBlock(IrBlockNode node)
    {
        if (_scope == null) _addedVariables.Clear();
        var previousScope = _scope;
        _scope = node.Scope;

        var result = base.VisitBlock(node);
        _scope = previousScope;
        return result;
    }

    public override IrNode VisitLocalVariableExpression(IrLocalVariableIdentifierExpressionNode node)
    {
        var id = GenerateVariableId(node.Name);
        return Scratch3CommandHelper.ItemAt(VariableValuesList,
            Scratch3CommandHelper.IndexOf(VariableNamesList, new IrConstantExpressionNode(TypedValue.String(id))));
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        var id = GenerateVariableId(node.Variable);

        if (!_addedVariables.Add(id))
            return new IrCommandSequenceNode([
                Scratch3CommandHelper.SetVariable(id, (IrExpressionNode)Visit(node.Expression))
            ]);

        return new IrCommandSequenceNode([
            new IrPushCommand(VariableNamesList,
                new IrConstantExpressionNode(TypedValue.String(id))),
            new IrPushCommand(VariableValuesList, (IrExpressionNode)Visit(node.Expression))
        ]);
    }
}
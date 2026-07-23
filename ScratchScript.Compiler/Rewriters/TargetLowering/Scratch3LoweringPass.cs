using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.AST.Representation.TargetSpecific;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Rewriters.Informational;
using ScratchScript.Compiler.Rewriters.Optimizations.HighLevel;
using ScratchScript.Compiler.Rewriters.Optimizations.LowLevel;
using ScratchScript.Compiler.TypeChecker;
using static ScratchScript.Compiler.Rewriters.TargetLowering.Scratch3CommandHelper;

namespace ScratchScript.Compiler.Rewriters.TargetLowering;

internal static class Scratch3CommandHelper
{
    public static readonly IrFunctionNode AllocateFrameFunction = new(true, new FunctionScope
    {
        FunctionName = ReservedNames.AllocateFrameFunction,
        ReturnType = ScratchType.Void,
        Arguments =
        [
            new ScratchScriptVariable(ReservedNames.ArgumentsCount, ScratchType.Number),
            new ScratchScriptVariable(ReservedNames.LocalsCount, ScratchType.Number)
        ],
        UseArgumentReporters = true,
        Body =
        [
            /*
             * FP = SP - argCount
             * repeat(localCount) push 0
             */
            new IrSetCommandNode(ReservedNames.FramePointer,
                new IrBinaryExpressionNode(IrBinaryOperator.Subtract,
                    LengthOf(ReservedNames.Stack),
                    new IrFunctionArgumentExpressionNode(ReservedNames.ArgumentsCount))),
            new IrRepeatCommandNode(new IrFunctionArgumentExpressionNode(ReservedNames.LocalsCount),
                new IrBlockNode([
                    new IrPushCommand(ReservedNames.Stack, new IrConstantExpressionNode(TypedValue.Number(0)))
                ]))
        ]
    });

    public static readonly IrFunctionNode CollapseFrameFunction = new(true, new FunctionScope
    {
        FunctionName = ReservedNames.CollapseFrameFunction,
        Arguments =
        [
            new ScratchScriptVariable(ReservedNames.HasReturnValue, ScratchType.Number)
        ],
        UseArgumentReporters = true,
        ReturnType = ScratchType.Void,
        Body =
        [
            /*
             * OFP = stack[FP]
             * SP = FP - 1
             * while(stack.length() != SP) pop
             * FP = OFP
             * if(hasReturn) stack.push(TRV)
             */
            new IrSetCommandNode(ReservedNames.OldFramePointer,
                ItemAt(ReservedNames.Stack,
                    new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer))),
            new IrWhileCommandNode(new IrBinaryExpressionNode(IrBinaryOperator.NotEqual,
                    LengthOf(ReservedNames.Stack),
                    new IrBinaryExpressionNode(IrBinaryOperator.Subtract,
                        new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer),
                        new IrConstantExpressionNode(TypedValue.Number(1)))),
                new IrBlockNode(new LoopScope
                    { Body = [new IrPopAtCommand(ReservedNames.Stack, LengthOf(ReservedNames.Stack))] })),
            new IrSetCommandNode(ReservedNames.FramePointer,
                new IrGlobalVariableIdentifierExpressionNode(ReservedNames.OldFramePointer)),
            new IrIfCommandNode(new IrBinaryExpressionNode(IrBinaryOperator.Equal,
                new IrFunctionArgumentExpressionNode(ReservedNames.HasReturnValue),
                new IrConstantExpressionNode(TypedValue.Number(1))), new IrBlockNode([
                new IrPushCommand(ReservedNames.Stack,
                    new IrGlobalVariableIdentifierExpressionNode(ReservedNames.TemporaryReturnValue))
            ]), null)
        ]
    });

    public static IrShadowExpressionNode IndexOf(string list, IrExpressionNode item) =>
        IrShadowBuilder
            .FromOpcode("data_itemnumoflist")
            .WithField("LIST", list)
            .WithInput("ITEM", item)
            .BuildExpression(ScratchType.Number);

    public static IrShadowExpressionNode ItemAt(string list, IrExpressionNode index) =>
        IrShadowBuilder
            .FromOpcode("data_itemoflist")
            .WithField("LIST", list)
            .WithInput("INDEX", index)
            .BuildExpression();

    public static IrShadowExpressionNode LengthOf(string list) =>
        IrShadowBuilder
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

    public static IrRawCommandNode StopThisScript() => IrShadowBuilder.FromOpcode("control_stop")
        .WithField("STOP_OPTION", "this script").BuildCommand();
}

public class Scratch3LoweringPass : IrRewriter
{
    private const string EventAllocationPerformedFlag = "SCRATCH3_EVENT_ALLOCATION_PERFORMED";
    private readonly List<IrFunctionNode> _pendingFunctions = [];

    public override IrNode VisitProgram(IrProgramNode node)
    {
        var program = (IrProgramNode)base.VisitProgram(node);

        if (!program.Functions.Any(b =>
                b is { FunctionScope.FunctionName: ReservedNames.AllocateFrameFunction }))
            _pendingFunctions.InsertRange(0, [
                AllocateFrameFunction, CollapseFrameFunction
            ]);

        return (program with { Functions = _pendingFunctions.Concat(program.Functions) }).WithFlag(
            EventAllocationPerformedFlag);
    }

    public override IrNode VisitEvent(IrEventNode node)
    {
        var result = (IrEventNode)base.VisitEvent(node);
        if (ProgramNode.Flags.Contains(EventAllocationPerformedFlag)) return result;

        var variableCountCalculator = new ScopeTotalVariableCountCalculationRewriter();
        variableCountCalculator.VisitBlock(result);

        var allocation = new IrCommandSequenceNode([
            new IrPushCommand(ReservedNames.Stack,
                new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer)),
            new IrCallFunctionCommandNode(ReservedNames.AllocateFrameFunction,
            [
                new IrConstantExpressionNode(TypedValue.Number(0)),
                new IrConstantExpressionNode(TypedValue.Number(variableCountCalculator.TotalVariableCount))
            ])
        ]);
        result.Scope.Body.Insert(0, allocation);
        result.Scope.Body.Add(new IrCallFunctionCommandNode(ReservedNames.CollapseFrameFunction,
        [
            new IrConstantExpressionNode(TypedValue.Number(0))
        ]));

        return result;
    }

    public override IrNode VisitLocalVariableIdentifierExpression(IrLocalVariableIdentifierExpressionNode node)
    {
        if (CurrentScope == null) throw new Exception("This node cannot be processed without a scope");
        return ItemAt(ReservedNames.Stack,
            GetLocalVariableExpression(node.Name)
        );
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        if (ReservedNames.GlobalVariables.Contains(node.Variable))
            return node with { Expression = (IrExpressionNode)Visit(node.Expression) };
        if (CurrentScope == null) throw new Exception("This node cannot be processed without a scope");
        return Replace(ReservedNames.Stack,
            GetLocalVariableExpression(node.Variable), (IrExpressionNode)Visit(node.Expression));
    }

    public override IrNode VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
    {
        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope == null)
            throw new Exception("This node cannot be processed without a scope");
        if (closestFunctionScope.UseArgumentReporters) return node;

        return ItemAt(ReservedNames.Stack,
            GetFunctionArgumentExpression(node.Name)
        );
    }

    public override IrNode VisitTernaryExpression(IrTernaryExpressionNode node) =>
        new IrComplexExpressionNode(new IrStackPointerExpressionNode(0),
            new IrIfCommandNode((IrExpressionNode)Visit(node.Condition),
                new IrBlockNode([new IrPushCommand(ReservedNames.Stack, (IrExpressionNode)Visit(node.TrueValue))],
                    CurrentScope),
                new IrBlockNode([new IrPushCommand(ReservedNames.Stack, (IrExpressionNode)Visit(node.FalseValue))],
                    CurrentScope)),
            new IrPopAtCommand(ReservedNames.Stack, LengthOf(ReservedNames.Stack)));

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        var function = ProgramNode.Functions.FirstOrDefault(f => f.FunctionScope.FunctionName == node.Function);
        if (function == null) throw new Exception();
        var visitedArguments =
            node.Arguments.Select(Visit).OfType<IrExpressionNode>();

        var commands = new List<IrCommandNode>();
        commands.Add(new IrPushCommand(ReservedNames.Stack,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer)));
        commands.AddRange(visitedArguments.Select(arg => new IrPushCommand(ReservedNames.Stack, arg)));
        commands.Add(new IrCallFunctionCommandNode(ReservedNames.AllocateFrameFunction,
        [
            new IrConstantExpressionNode(TypedValue.Number(function.FunctionScope.Arguments.Count)),
            new IrConstantExpressionNode(TypedValue.Number(function.FunctionScope.Variables.Count))
        ]));
        commands.Add(new IrCallFunctionCommandNode(node.Function, []));
        return new IrComplexExpressionNode(
            new IrStackPointerExpressionNode(0),
            new IrCommandSequenceNode(commands),
            new IrPopAtCommand(ReservedNames.Stack, LengthOf(ReservedNames.Stack)));
    }

    public override IrNode VisitFunctionReturnCommandNode(IrReturnCommandNode node)
    {
        var commands = new List<IrCommandNode>();
        if (node.ReturnValue != null)
            commands.Add(
                new IrSetCommandNode(ReservedNames.TemporaryReturnValue, (IrExpressionNode)Visit(node.ReturnValue)));
        commands.AddRange([
            new IrCallFunctionCommandNode(ReservedNames.CollapseFrameFunction,
            [
                new IrConstantExpressionNode(TypedValue.Number(node.ReturnValue != null ? 1 : 0))
            ]),
            StopThisScript()
        ]);
        return new IrCommandSequenceNode(commands);
    }

    private IrBinaryExpressionNode GetLocalVariableExpression(string name)
    {
        if (CurrentScope == null) throw new Exception();

        var path = CurrentScope.GetPathToTopmostParent();
        var index = 0;
        foreach (var scope in path.AsEnumerable().Reverse())
        {
            var localIndex = scope.Variables.FindIndex(v => v.Name == name);
            index += localIndex == -1 ? scope.Variables.Count : localIndex + 1;
            if (localIndex != -1) break;
        }

        var offset = CurrentScope is FunctionScope functionScope ? functionScope.Arguments.Count : 0;
        return new IrBinaryExpressionNode(IrBinaryOperator.Add,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer),
            new IrConstantExpressionNode(TypedValue.Number(offset + index)));
    }

    private IrBinaryExpressionNode GetFunctionArgumentExpression(string name)
    {
        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope == null) throw new Exception();
        var index = closestFunctionScope.Arguments.FindIndex(v => v.Name == name) + 1;
        return new IrBinaryExpressionNode(IrBinaryOperator.Add,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer),
            new IrConstantExpressionNode(TypedValue.Number(index)));
    }
}
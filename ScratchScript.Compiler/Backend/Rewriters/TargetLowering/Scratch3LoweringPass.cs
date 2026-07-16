using ScratchScript.Compiler.Backend.Information;
using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;
using static ScratchScript.Compiler.Backend.Rewriters.TargetLowering.Scratch3CommandHelper;

namespace ScratchScript.Compiler.Backend.Rewriters.TargetLowering;

internal static class Scratch3CommandHelper
{
    public static readonly IrFunctionNode AllocateFrameFunction = new(true, new FunctionScope
    {
        Id = ReservedNames.AllocateFrameFunction,
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
        Id = ReservedNames.CollapseFrameFunction,
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
                    new IrConstantExpressionNode(TypedValue.Number(1)))), new IrBlockNode([
                new IrPopAtCommand(ReservedNames.Stack, LengthOf(ReservedNames.Stack))
            ])),
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

    public static IrShadowExpressionNode IndexOf(string list, IrExpressionNode item)
    {
        return IrShadowBuilder
            .FromOpcode("data_itemnumoflist")
            .WithField("LIST", list)
            .WithInput("ITEM", item)
            .BuildExpression(ScratchType.Number);
    }

    public static IrShadowExpressionNode ItemAt(string list, IrExpressionNode index)
    {
        return IrShadowBuilder
            .FromOpcode("data_itemoflist")
            .WithField("LIST", list)
            .WithInput("INDEX", index)
            .BuildExpression();
    }

    public static IrShadowExpressionNode LengthOf(string list)
    {
        return IrShadowBuilder
            .FromOpcode("data_lengthoflist")
            .WithField("LIST", list)
            .BuildExpression(ScratchType.Number);
    }

    public static IrRawCommandNode Replace(string list, IrExpressionNode index, IrExpressionNode value)
    {
        return IrShadowBuilder
            .FromOpcode("data_replaceitemoflist")
            .WithField("LIST", list)
            .WithInput("INDEX", index)
            .WithInput("ITEM", value)
            .BuildCommand();
    }

    public static IrRawCommandNode StopThisScript()
    {
        return IrShadowBuilder.FromOpcode("control_stop").WithField("STOP_OPTION", "this script").BuildCommand();
    }
}

public class Scratch3LoweringPass : IrRewriter
{
    private const string EventAllocationPerformedFlag = "SCRATCH3_EVENT_ALLOCATION_PERFORMED";

    private Scope? _scope;
    private IEnumerable<IrFunctionNode> _functions => _program.Blocks.OfType<IrFunctionNode>();
    private IrProgramNode _program;

    public override IrNode VisitProgram(IrProgramNode node)
    {
        _program = node;
        var program = (IrProgramNode)base.VisitProgram(node);

        if (!program.Blocks.Any(b =>
                b is IrFunctionNode { FunctionScope.Id: ReservedNames.AllocateFrameFunction }))
            program = program with
            {
                Blocks = new List<IrBlockNode>([
                    AllocateFrameFunction, CollapseFrameFunction
                ]).Concat(program.Blocks).ToList()
            };

        program.Flags[EventAllocationPerformedFlag] = true;
        return program;
    }

    public override IrNode VisitBlock(IrBlockNode node)
    {
        var previousScope = _scope;
        _scope = node.Scope;

        var result = (IrBlockNode)base.VisitBlock(node);
        _scope = previousScope;
        return result;
    }

    public override IrNode VisitEvent(IrEventNode node)
    {
        var result = (IrEventNode)base.VisitEvent(node);
        if (_program.Flags.ContainsKey(EventAllocationPerformedFlag)) return result;

        var allocation = new IrCommandSequenceNode([
            new IrPushCommand(ReservedNames.Stack,
                new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer)),
            new IrCallFunctionCommandNode(ReservedNames.AllocateFrameFunction,
                new Dictionary<string, IrExpressionNode>
                {
                    {
                        ReservedNames.ArgumentsCount,
                        new IrConstantExpressionNode(TypedValue.Number(0))
                    },
                    {
                        ReservedNames.LocalsCount,
                        new IrConstantExpressionNode(TypedValue.Number(node.Scope.Variables.Count))
                    }
                })
        ]);
        result.Scope.Body.Insert(0, allocation);
        result.Scope.Body.Add(new IrCallFunctionCommandNode(ReservedNames.CollapseFrameFunction,
            new Dictionary<string, IrExpressionNode>
            {
                {
                    ReservedNames.HasReturnValue,
                    new IrConstantExpressionNode(TypedValue.Number(0))
                }
            }));

        return result;
    }

    public override IrNode VisitLocalVariableExpression(IrLocalVariableIdentifierExpressionNode node)
    {
        if (_scope == null) throw new Exception("This node cannot be processed without a scope");
        return ItemAt(ReservedNames.Stack,
            GetLocalVariableExpression(node.Name)
        );
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        if (ReservedNames.GlobalVariables.Contains(node.Variable))
            return node with { Expression = (IrExpressionNode)Visit(node.Expression) };
        if (_scope == null) throw new Exception("This node cannot be processed without a scope");
        return Replace(ReservedNames.Stack,
            GetLocalVariableExpression(node.Variable), (IrExpressionNode)Visit(node.Expression));
    }

    public override IrNode VisitFunctionArgumentExpressionNode(IrFunctionArgumentExpressionNode node)
    {
        if (_scope is not FunctionScope functionScope)
            throw new Exception("This node cannot be processed without a scope");
        if (functionScope.UseArgumentReporters) return node;

        return ItemAt(ReservedNames.Stack,
            GetFunctionArgumentExpression(node.Name)
        );
    }

    public override IrNode VisitFunctionCallExpressionNode(IrFunctionCallExpressionNode node)
    {
        var function = _functions.FirstOrDefault(f => f.FunctionScope.FunctionName == node.Function);
        if (function == null) throw new Exception();
        var visitedArguments =
            node.Arguments.ToDictionary(kvp => kvp.Key, kvp => (IrExpressionNode)Visit(kvp.Value));

        var commands = new List<IrCommandNode>();
        commands.Add(new IrPushCommand(ReservedNames.Stack,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer)));
        commands.AddRange(visitedArguments.Select(kvp => new IrPushCommand(ReservedNames.Stack, kvp.Value)));
        commands.Add(new IrCallFunctionCommandNode(ReservedNames.AllocateFrameFunction,
            new Dictionary<string, IrExpressionNode>
            {
                {
                    ReservedNames.ArgumentsCount,
                    new IrConstantExpressionNode(TypedValue.Number(function.FunctionScope.Arguments.Count))
                },
                {
                    ReservedNames.LocalsCount,
                    new IrConstantExpressionNode(TypedValue.Number(function.FunctionScope.Variables.Count))
                }
            }));
        commands.Add(new IrCallFunctionCommandNode(node.Function, []));
        return new IrComplexExpressionNode(
            ItemAt(ReservedNames.Stack,
                LengthOf(ReservedNames.Stack)),
            new IrCommandSequenceNode(commands),
            new IrPopAtCommand(ReservedNames.Stack, LengthOf(ReservedNames.Stack)));
    }

    public override IrNode VisitFunctionReturnCommandNode(IrFunctionReturnCommandNode node)
    {
        var commands = new List<IrCommandNode>();
        if (node.ReturnValue != null)
            commands.Add(new IrSetCommandNode(ReservedNames.TemporaryReturnValue, node.ReturnValue));
        commands.AddRange([
            new IrCallFunctionCommandNode(ReservedNames.CollapseFrameFunction,
                new Dictionary<string, IrExpressionNode>
                {
                    {
                        ReservedNames.HasReturnValue,
                        new IrConstantExpressionNode(TypedValue.Number(node.ReturnValue != null ? 1 : 0))
                    }
                }),
            StopThisScript()
        ]);
        return new IrCommandSequenceNode(commands);
    }

    private IrBinaryExpressionNode GetLocalVariableExpression(string name)
    {
        if (_scope == null) throw new Exception();
        var index = _scope.Variables.FindIndex(v => v.Name == name) + 1;
        var offset = _scope is FunctionScope functionScope ? functionScope.Arguments.Count : 0;
        return new IrBinaryExpressionNode(IrBinaryOperator.Add,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer),
            new IrConstantExpressionNode(TypedValue.Number(offset + index)));
    }

    private IrBinaryExpressionNode GetFunctionArgumentExpression(string name)
    {
        if (_scope is not FunctionScope functionScope) throw new Exception();
        var index = functionScope.Arguments.FindIndex(v => v.Name == name) + 1;
        return new IrBinaryExpressionNode(IrBinaryOperator.Add,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer),
            new IrConstantExpressionNode(TypedValue.Number(index)));
    }
}
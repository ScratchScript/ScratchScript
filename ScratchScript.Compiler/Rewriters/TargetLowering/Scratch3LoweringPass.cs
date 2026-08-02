using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Rewriters.Informational;
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
    }, []);

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
    }, []);

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
    private const string FunctionAllocationPerformedFlag = "SCRATCH3_FUNCTION_ALLOCATION_PERFORMED";
    private const string NativeFunctionCallFlag = "SCRATCH3_NATIVE_FUNCTION_CALL";
    private const string EnumsSerializedFlag = "SCRATCH3_ENUMS_SERIALIZED";

    private const string SkipAllocationKey = "packFrameAllocationFunctions";
    private const string SkipEnumSerializationKey = "enumSerialization";

    private readonly List<IrNode> _pendingBlocks = [];

    public override IrNode VisitProgram(IrProgramNode node)
    {
        _pendingBlocks.Clear();
        var program = (IrProgramNode)base.VisitProgram(node);

        if (!program.HasAttributeWithArgument(ProgramAttributes.SkipCompilerFeature, SkipAllocationKey) &&
            !program.Functions.Any(b =>
                b is { FunctionScope.FunctionName: ReservedNames.AllocateFrameFunction }))
            _pendingBlocks.InsertRange(0, [
                AllocateFrameFunction, CollapseFrameFunction
            ]);

        if (!program.HasAttributeWithArgument(ProgramAttributes.SkipCompilerFeature, SkipEnumSerializationKey) &&
            !program.Flags.Contains(EnumsSerializedFlag))
            program = SerializeEnums(program);

        return (program with { TopLevelNodes = _pendingBlocks.Concat(program.TopLevelNodes).ToList() }).WithFlag(
            EventAllocationPerformedFlag);
    }

    // TODO: this can be removed if static data can be passed to the project emitter from compiler passes
    private static IrProgramNode SerializeEnums(IrProgramNode program)
    {
        var blocks = new List<IrNode>(program.TopLevelNodes);
        if (blocks.FirstOrDefault(e => e is IrEventNode { Type: "start" }) is not IrEventNode startEvent)
            return program;

        var enums = blocks.OfType<IrEnumNode>().ToList();
        if (enums.Count == 0) return program;

        var body = new List<IrCommandNode> { new IrPopAllCommand(ReservedNames.Enums) };
        foreach (var en in enums)
        {
            body.AddRange(en.Entries.Keys.Select(key =>
                new IrPushCommand(ReservedNames.Enums, new IrConstantExpressionNode(TypedValue.String(key)))));
            body.AddRange(en.Entries.Values.Select(value =>
                new IrPushCommand(ReservedNames.Enums, value ?? throw new ArgumentNullException(nameof(value)))));
        }

        var initFunction = new IrFunctionNode(true,
            new FunctionScope
                { UseArgumentReporters = true, FunctionName = ReservedNames.InitEnumsFunction, Body = body }, []);

        blocks.Add(initFunction);
        startEvent.Scope.Body.Insert(0, new IrCallFunctionCommandNode(ReservedNames.InitEnumsFunction, []));
        return (program with { TopLevelNodes = blocks }).WithFlag(EnumsSerializedFlag);
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
            ]).WithFlag(NativeFunctionCallFlag)
        ]);
        result.Scope.Body.Insert(0, allocation);
        result.Scope.Body.Add(new IrCallFunctionCommandNode(ReservedNames.CollapseFrameFunction,
        [
            new IrConstantExpressionNode(TypedValue.Number(0))
        ]).WithFlag(NativeFunctionCallFlag));

        return result;
    }

    public override IrNode VisitFunction(IrFunctionNode node)
    {
        var result = (IrFunctionNode)base.VisitFunction(node);
        if (result.Flags.Contains(FunctionAllocationPerformedFlag)) return result;
        // native stuff we don't touch
        if (IsFunctionSpecial(result.FunctionScope))
            return result.WithFlag(FunctionAllocationPerformedFlag);

        var variableCountCalculator = new ScopeTotalVariableCountCalculationRewriter();
        variableCountCalculator.VisitBlock(result);

        result.FunctionScope.Body.Insert(0, new IrCallFunctionCommandNode(ReservedNames.AllocateFrameFunction,
        [
            new IrConstantExpressionNode(TypedValue.Number(result.FunctionScope.Arguments.Count)),
            new IrConstantExpressionNode(TypedValue.Number(variableCountCalculator.TotalVariableCount))
        ]).WithFlag(NativeFunctionCallFlag));
        if (!result.FunctionScope.HasReturn)
            result.FunctionScope.Body.Add(new IrCallFunctionCommandNode(ReservedNames.CollapseFrameFunction,
            [
                new IrConstantExpressionNode(TypedValue.Number(0))
            ]).WithFlag(NativeFunctionCallFlag));

        return result.WithFlag(FunctionAllocationPerformedFlag);
    }

    public override IrNode VisitLocalVariableIdentifierExpression(IrLocalVariableIdentifierExpressionNode node)
    {
        if (CurrentScope == null) throw new Exception("This node cannot be processed without a scope");
        return ItemAt(ReservedNames.Stack,
            GetLocalVariableExpression(node.Name)
        ).WithInferredType(node.InferredType);
    }

    public override IrNode VisitSetCommand(IrSetCommandNode node)
    {
        if (ReservedNames.GlobalVariables.Contains(node.Variable))
            return node with { Expression = (IrExpressionNode)Visit(node.Expression) };
        if (CurrentScope == null) throw new Exception("This node cannot be processed without a scope");
        return Replace(ReservedNames.Stack,
            GetLocalVariableExpression(node.Variable), (IrExpressionNode)Visit(node.Expression));
    }

    public override IrNode VisitFunctionArgumentExpression(IrFunctionArgumentExpressionNode node)
    {
        var closestFunctionScope = CurrentScope?.GetClosestFunctionScope();
        if (closestFunctionScope == null)
            throw new Exception("This node cannot be processed without a scope");
        if (IsFunctionSpecial(closestFunctionScope)) return node;

        return ItemAt(ReservedNames.Stack,
            GetFunctionArgumentExpression(node.Name)
        ).WithInferredType(node.InferredType);
    }

    public override IrNode VisitTernaryExpression(IrTernaryExpressionNode node) =>
        new IrComplexExpressionNode(new IrStackPointerExpressionNode(0),
            new IrIfCommandNode((IrExpressionNode)Visit(node.Condition),
                new IrBlockNode([new IrPushCommand(ReservedNames.Stack, (IrExpressionNode)Visit(node.TrueValue))],
                    CurrentScope),
                new IrBlockNode([new IrPushCommand(ReservedNames.Stack, (IrExpressionNode)Visit(node.FalseValue))],
                    CurrentScope)),
            new IrPopAtCommand(ReservedNames.Stack, LengthOf(ReservedNames.Stack))).WithInferredType(node.InferredType);

    public override IrNode VisitFunctionCallExpression(IrFunctionCallExpressionNode node)
    {
        var visitedArguments =
            node.Arguments.Select(Visit).OfType<IrExpressionNode>().ToList();

        if (ReservedNames.GlobalCallableFunctions.Contains(node.Function))
            return node with { Arguments = visitedArguments };

        var function = ProgramNode.Functions.FirstOrDefault(f => f.FunctionScope.FunctionName == node.Function);
        if (function == null) throw new Exception();

        var commands = new List<IrCommandNode>();
        commands.Add(new IrPushCommand(ReservedNames.Stack,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer)));
        commands.AddRange(visitedArguments.Select(arg => new IrPushCommand(ReservedNames.Stack, arg)));
        commands.Add(new IrCallFunctionCommandNode(node.Function, []).WithFlag(NativeFunctionCallFlag));
        return new IrComplexExpressionNode(
            new IrStackPointerExpressionNode(0),
            new IrCommandSequenceNode(commands),
            new IrPopAtCommand(ReservedNames.Stack, LengthOf(ReservedNames.Stack)));
    }

    public override IrNode VisitCallFunctionCommand(IrCallFunctionCommandNode node)
    {
        var visitedArguments =
            node.Arguments.Select(Visit).OfType<IrExpressionNode>().ToList();
        if (node.Flags.Contains(NativeFunctionCallFlag)) return node with { Arguments = visitedArguments };

        var function = ProgramNode.Functions.FirstOrDefault(f => f.FunctionScope.FunctionName == node.Function);
        if (function == null) throw new Exception();
        if (IsFunctionSpecial(function.FunctionScope))
            return node with { Arguments = visitedArguments };

        var commands = new List<IrCommandNode>();
        commands.Add(new IrPushCommand(ReservedNames.Stack,
            new IrGlobalVariableIdentifierExpressionNode(ReservedNames.FramePointer)));
        commands.AddRange(visitedArguments.Select(arg => new IrPushCommand(ReservedNames.Stack, arg)));
        commands.Add(new IrCallFunctionCommandNode(node.Function, []).WithFlag(NativeFunctionCallFlag));
        return new IrCommandSequenceNode(commands);
    }

    public override IrNode VisitFunctionReturnCommand(IrReturnCommandNode node)
    {
        var commands = new List<IrCommandNode>();
        if (node.ReturnValue != null)
            commands.Add(
                new IrSetCommandNode(ReservedNames.TemporaryReturnValue, (IrExpressionNode)Visit(node.ReturnValue)));
        commands.AddRange([
            new IrCallFunctionCommandNode(ReservedNames.CollapseFrameFunction,
            [
                new IrConstantExpressionNode(TypedValue.Number(node.ReturnValue != null ? 1 : 0))
            ]).WithFlag(NativeFunctionCallFlag),
            StopThisScript()
        ]);
        return new IrCommandSequenceNode(commands);
    }

    public override IrNode VisitMemberPropertyExpression(IrMemberPropertyExpressionNode node)
    {
        var member = (IrExpressionNode)Visit(node.Member);
        switch (member)
        {
            case IrTypeReferenceExpressionNode typeref:
            {
                switch (typeref.InnerNode)
                {
                    case IrEnumNode enumNode:
                        if (ProgramNode.HasAttributeWithArgument(ProgramAttributes.SkipCompilerFeature,
                                SkipEnumSerializationKey))
                            return node with { Member = member };

                        var enumIndex = ProgramNode.Enums.ToList().FindIndex(e => e.Name == enumNode.Name);
                        var memberIndex = enumNode.Entries.Keys.ToList().IndexOf(node.Property);
                        if (enumIndex == -1 || memberIndex == -1) throw new Exception();

                        return new IrConstantExpressionNode(
                                TypedValue.Number(memberIndex))
                            .WithInferredType(node.InferredType);
                }

                break;
            }
            default:
            {
                if (member.InferredType is EnumScratchType enumType)
                {
                    if (ProgramNode.HasAttributeWithArgument(ProgramAttributes.SkipCompilerFeature,
                            SkipEnumSerializationKey))
                        return node with { Member = member };

                    var enums = ProgramNode.Enums.ToList();
                    var enumIndex = enums.FindIndex(e => e.Name == enumType.Name);
                    if (enumIndex == -1) throw new Exception();

                    var offset = 1; // lists are 1-indexed
                    for (var i = 0; i < enumIndex; i++)
                        offset += enums[i].Entries.Count * 2;
                    if (node.Property == "value") offset += enums[enumIndex].Entries.Count;

                    return ItemAt(ReservedNames.Enums,
                            new IrBinaryExpressionNode(IrBinaryOperator.Add,
                                new IrConstantExpressionNode(TypedValue.Number(offset)), member))
                        .WithInferredType(ScratchType.String);
                }

                break;
            }
        }

        return node with { Member = member };
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

    private bool IsFunctionSpecial(FunctionScope scope)
    {
        var function = ProgramNode.Functions.FirstOrDefault(f => f.FunctionScope.Id == scope.Id);
        if (function == null) return true;
        return function.Attributes.Any(a => a.Name == FunctionAttributes.AlwaysInlineFunction) ||
               scope.UseArgumentReporters;
    }
}
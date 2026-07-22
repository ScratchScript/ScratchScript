namespace ScratchScript.Compiler.AST.Information;

public static class ReservedNames
{
    public const string Stack = "__Stack";
    public const string FramePointer = "__FP";
    public const string OldFramePointer = "__OFP";
    public const string TemporaryReturnValue = "__TRV";

    public const string AllocateFrameFunction = "__allocateFrame";
    public const string CollapseFrameFunction = "__collapseFrame";
    public const string ArgumentsCount = "__argCount";
    public const string LocalsCount = "__localCount";
    public const string HasReturnValue = "__hasReturn";

    // TODO: this is for callbacks, which aren't supported yet
    public const string DispatchFunction = "__dispatch";
    public const string RawStatementFunction = "__raw";
    public const string RawExpressionFunction = "__raw_expr";

    public const string WhileBreak = "__B";
    public const string WhileMainFunction = "__while_main";
    public const string WhileBodyFunction = "__while_body";

    // TODO: reserve all identifiers starting with "__" for the compiler? (toggleable?)
    public static readonly List<string> DisallowedIdentifiers =
    [
        Stack, FramePointer, OldFramePointer, TemporaryReturnValue, AllocateFrameFunction,
        CollapseFrameFunction, DispatchFunction, ArgumentsCount, LocalsCount, RawStatementFunction,
        RawExpressionFunction, WhileMainFunction, WhileBodyFunction
    ];

    public static readonly List<string> GlobalCallableFunctions = [RawStatementFunction, RawExpressionFunction];
    public static readonly List<string> GlobalVariables = [FramePointer, OldFramePointer, TemporaryReturnValue, WhileBreak];
}
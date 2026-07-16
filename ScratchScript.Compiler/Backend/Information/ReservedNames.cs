namespace ScratchScript.Compiler.Backend.Information;

public class ReservedNames
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

    public static readonly List<string> DisallowedIdentifiers =
    [
        Stack, FramePointer, OldFramePointer, TemporaryReturnValue, AllocateFrameFunction,
        CollapseFrameFunction, DispatchFunction, ArgumentsCount, LocalsCount
    ];

    public static readonly List<string> GlobalVariables = [FramePointer, OldFramePointer, TemporaryReturnValue];
}
namespace ScratchScript.Compiler.Frontend;

public static class BackendHelper
{
    public const string StackListName = "__Stack";
    
    public static string PushToStack(object value) => $"push {StackListName} {value}";
    public static string PopStack() => $"pop {StackListName}";
}
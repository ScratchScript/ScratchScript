using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3FunctionHandler : IFunctionHandler
{
    public TypedValue GetArgument(ref Scope scope, string name)
    {
        if (scope is not Scratch3FunctionScope function)
            throw new Exception("Expected a Scratch3FunctionScope for GetArgument.");

        var index = function.Arguments.FindIndex(arg => arg.Name == name);

        if (index == -1)
            throw new Exception(
                $"The function \"{function.FunctionName}\" does not have an argument with the name \"{name}\".");

        return new TypedValue(Scratch3Helper.ItemOf(Scratch3Helper.StackList, index.ToString()),
            function.Arguments[index].Type);
    }

    public void HandleFunctionExit(ref Scope scope, ExpressionValue? value)
    {
        if (scope is not Scratch3FunctionScope function)
            throw new Exception("Expected a Scratch3FunctionScope for GetArgument.");

        var debt = value?.StackDebt ?? 0;
        if (!string.IsNullOrEmpty(value?.Dependencies)) scope.Content.Add(value.Dependencies);
        if (value != null)
            function.Content.Add(Scratch3Helper.PushAt(Scratch3Helper.StackList,
                (debt + function.Arguments.Count + 1).ToString(), value.Value!));
        if (!string.IsNullOrEmpty(value?.Cleanup)) scope.Content.Add(value.Cleanup);
        function.Content.Add(Scratch3Helper.Repeat(function.Arguments.Count.ToString(),
            Scratch3Helper.Pop(Scratch3Helper.StackList)));
        function.Content.Add(Scratch3Helper.StopThisScript());
    }
}
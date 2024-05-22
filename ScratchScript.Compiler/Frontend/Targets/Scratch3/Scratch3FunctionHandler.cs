using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3FunctionHandler : IFunctionHandler
{
    public TypedValue GetArgument(ref IScope scope, string name)
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

    public void HandleFunctionExit(ref IScope scope, ExpressionValue? value)
    {
        if (scope is not Scratch3FunctionScope function)
            throw new Exception("Expected a Scratch3FunctionScope for GetArgument.");

        // return statement structure:
        // {dependencies needed for the return value}
        // {push return value to (stack debt + argument count + 1)}
        // {cleanup of the return value}
        // {pop arguments from the stack}
        // {stop the script}

        if (value?.Dependencies != null) scope.Content.AddRange(value.Dependencies);
        if (value != null)
            function.Content.Add(Scratch3Helper.PushAt(Scratch3Helper.StackList,
                (function.StackDebt + function.Arguments.Count + 1).ToString(), value.Value!));
        if (value?.Cleanup != null) scope.Content.AddRange(value.Cleanup);
        function.Content.Add(Scratch3Helper.Repeat(function.Arguments.Count.ToString(),
            Scratch3Helper.Pop(Scratch3Helper.StackList)));
        function.Content.Add(Scratch3Helper.StopThisScript());
    }

    public ExpressionValue? HandleFunctionCall(ref IScope _scope, IFunctionScope function,
        IEnumerable<ExpressionValue> arguments)
    {
        if (_scope is not Scratch3Scope scope)
            throw new Exception("Expected a Scratch3Scope for HandleFunctionCall.");

        var dependencies = new List<string>();
        var cleanup = new List<string>();
        foreach (var argument in arguments.Reverse())
        {
            if (argument.Value == null) throw new Exception("A function argument had a null value.");

            dependencies.AddRange(argument.Dependencies ?? []);
            dependencies.Add(Scratch3Helper.PushAt(Scratch3Helper.StackList, "1", argument.Value));
            cleanup.AddRange(argument.Cleanup ?? []);
        }

        dependencies.Add(Scratch3Helper.CallFunction(function.FunctionName));
        cleanup.Add(Scratch3Helper.PopAt(Scratch3Helper.StackList, "1"));
        scope.StackDebt++;

        if (function.ReturnType == ScratchType.Void)
        {
            scope.Content.AddRange(dependencies);
            scope.Content.AddRange(cleanup);
            return null;
        }

        return new ExpressionValue(Scratch3Helper.ItemOf(Scratch3Helper.StackList, scope.StackDebt.ToString()),
            function.ReturnType, dependencies, cleanup);
    }
}
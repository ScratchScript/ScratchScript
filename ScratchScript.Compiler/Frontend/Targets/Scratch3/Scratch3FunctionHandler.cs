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
}
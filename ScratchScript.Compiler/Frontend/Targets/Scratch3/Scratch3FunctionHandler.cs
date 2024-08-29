using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3FunctionHandler : IFunctionHandler
{
    public TypedValue GetArgument(IScope scope, string name)
    {
        if (scope is not Scratch3FunctionScope function)
            throw new Exception("Expected a Scratch3FunctionScope for GetArgument.");

        var index = function.Arguments.FindIndex(arg => arg.Name == name);
        if (index == -1)
            throw new Exception(
                $"The function \"{function.FunctionName}\" does not have an argument with the name \"{name}\".");

        if (function.Inlined)
            return new ExpressionValue(GenerateInlinedFunctionArgumentId(function.Id, name),
                function.Arguments[index].Type);

        return new IdentifierExpressionValue(IdentifierType.FunctionArgument, name, function,
            Scratch3Helper.ItemOf(Scratch3Helper.StackList,
                $"- {Scratch3Helper.StackPointerReporter} {function.Arguments.Count - index - 1}"),
            function.Arguments[index].Type);
    }

    public TypedValue HandleFunctionArgumentAssignment(IScope scope, string name, ExpressionValue value)
    {
        if (scope is not Scratch3FunctionScope function)
            throw new Exception("Expected a Scratch3FunctionScope for HandleFunctionArgumentAssignment.");
        if (function.Inlined)
            throw new Exception("Cannot assign new values to inlined function arguments.");

        var index = function.Arguments.FindIndex(arg => arg.Name == name);
        if (index == -1)
            throw new Exception(
                $"The function \"{function.FunctionName}\" does not have an argument with the name \"{name}\".");

        return new StatementValue([
            Scratch3Helper.Replace(Scratch3Helper.StackList,
                $"- {Scratch3Helper.StackPointerReporter} {function.Arguments.Count - index - 1}",
                value.Value!)
        ], value.Dependencies, value.Cleanup);
    }

    public void HandleFunctionExit(IScope scope, ExpressionValue? value)
    {
        if (scope is not Scratch3FunctionScope function)
            throw new Exception("Expected a Scratch3FunctionScope for GetArgument.");

        // return statement structure:
        // {dependencies needed for the return value}
        // {push return value to (stack pointer + 1)}
        // {cleanup of the return value}
        // {pop arguments from the stack}
        // {stop the script}

        if (function.Inlined)
        {
            function.InlinedReturnValue = value;
        }
        else
        {
            if (value?.Dependencies != null) scope.Content.AddRange(value.Dependencies);
            if (value != null)
                function.Content.Add(Scratch3Helper.PushAt(Scratch3Helper.StackList,
                    $"+ {Scratch3Helper.StackPointerReporter} 1", value.Value!));
            if (value?.Cleanup != null) scope.Content.AddRange(value.Cleanup);
            function.Content.Add(Scratch3Helper.Repeat(function.Arguments.Count.ToString(),
                Scratch3Helper.PopAt(Scratch3Helper.StackList,
                    $"- {Scratch3Helper.StackPointerReporter} {function.Arguments.Count - 1}")));
            function.Content.Add(Scratch3Helper.StopThisScript());
        }
    }

    public ExpressionValue? HandleFunctionCall(IScope _scope, IFunctionScope function,
        IEnumerable<ExpressionValue> arguments)
    {
        if (_scope is not Scratch3Scope scope)
            throw new Exception("Expected a Scratch3Scope for HandleFunctionCall.");

        var dependencies = new List<string>();
        var cleanup = new List<string>();

        // TODO: add documentation to this process later
        if (function.Inlined)
        {
            var argumentsList = arguments.ToList();
            var returnValue = function.InlinedReturnValue;

            void ReplaceArgument(string id, string with)
            {
                for (var lineIndex = 0; lineIndex < function.Content.Count; lineIndex++)
                    function.Content[lineIndex] = function.Content[lineIndex].Replace(id, with);

                if (returnValue?.Value != null)
                    returnValue = returnValue with
                    {
                        Value = returnValue.Value.ToString()!.Replace(id, with),
                        Dependencies = returnValue.Dependencies?.Select(line => line.Replace(id, with)),
                        Cleanup = returnValue.Cleanup?.Select(line => line.Replace(id, with))
                    };
            }

            for (var idx = 0; idx < argumentsList.Count; idx++)
            {
                var argument = argumentsList[idx];
                if (argument.Value == null) throw new Exception("A function argument had a null value.");

                dependencies.AddRange(argument.Dependencies ?? []);
                cleanup.AddRange(argument.Cleanup ?? []);

                ReplaceArgument(GenerateInlinedFunctionArgumentId(function.Id, function.Arguments[idx].Name),
                    argument.Value.ToString()!);
            }

            ReplaceArgument(
                GenerateInlinedFunctionArgumentId(function.Id, Scratch3Helper.IntermediateStackPointerReporter),
                scope.TotalIntermediateStackCount.ToString());

            if (function.ReturnType == ScratchType.Void)
            {
                scope.Content.AddRange([..dependencies, ..cleanup, ..function.Content]);
                return null;
            }

            returnValue = returnValue! with
            {
                Dependencies = function.Content.ConcatNullable(returnValue.Dependencies)
            };
            return returnValue;
        }

        var push = new List<string>();
        foreach (var argument in arguments)
        {
            if (argument.Value == null) throw new Exception("A function argument had a null value.");

            dependencies.AddRange(argument.Dependencies ?? []);
            cleanup.AddRange(argument.Cleanup ?? []);
            push.Add(Scratch3Helper.Push(Scratch3Helper.StackList, argument.Value));
        }

        push.Reverse();
        dependencies.AddRange(push);

        dependencies.Add(Scratch3Helper.CallFunction(function.FunctionName));
        dependencies.Add(Scratch3Helper.Push(Scratch3Helper.IntermediateStackList,
            Scratch3Helper.ItemOf(Scratch3Helper.StackList, Scratch3Helper.LengthOf(Scratch3Helper.StackList))));
        dependencies.Add(Scratch3Helper.PopAt(Scratch3Helper.StackList,
            Scratch3Helper.LengthOf(Scratch3Helper.StackList)));
        cleanup.Add(Scratch3Helper.Pop(Scratch3Helper.IntermediateStackList));
        scope.IntermediateStackCount++;

        if (function.ReturnType == ScratchType.Void)
        {
            scope.Content.AddRange(dependencies);
            scope.Content.AddRange(cleanup);
            return null;
        }

        var resultIndex = scope is Scratch3FunctionScope functionScope
            ? $"+ {(functionScope.Inlined ? GenerateInlinedFunctionArgumentId(functionScope.Id, Scratch3Helper.IntermediateStackPointerReporter) : Scratch3Helper.IntermediateStackPointerReporter)} {functionScope.TotalIntermediateStackCount}"
            : scope.TotalIntermediateStackCount.ToString();

        return new ExpressionValue(
            Scratch3Helper.ItemOf(Scratch3Helper.IntermediateStackList, resultIndex),
            function.ReturnType, dependencies, cleanup);
    }

    private static string GenerateInlinedFunctionArgumentId(string id, string name)
    {
        return $"__function_argument_{id}_{name}__";
    }
}
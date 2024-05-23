using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IFunctionHandler
{
    public TypedValue GetArgument(ref IScope scope, string name);
    public void HandleFunctionArgumentAssignment(ref IScope scope, string name, ExpressionValue value);
    public void HandleFunctionExit(ref IScope scope, ExpressionValue? value);

    public ExpressionValue? HandleFunctionCall(ref IScope scope, IFunctionScope function,
        IEnumerable<ExpressionValue> arguments);
}
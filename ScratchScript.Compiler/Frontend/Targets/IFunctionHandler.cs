using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IFunctionHandler
{
    public TypedValue GetArgument(IScope scope, string name);
    public TypedValue HandleFunctionArgumentAssignment(IScope scope, string name, ExpressionValue value);
    public void HandleFunctionExit(IScope scope, ExpressionValue? value);

    public ExpressionValue? HandleFunctionCall(IScope scope, IFunctionScope function,
        IEnumerable<ExpressionValue> arguments);
}
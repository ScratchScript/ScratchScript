using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IFunctionHandler
{
    public TypedValue GetArgument(ref Scope scope, string name);
    public void HandleFunctionExit(ref Scope scope, ExpressionValue? value);
}
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IFunctionHandler
{
    public ExpressionValue GetArgument(ref Scope scope, string name);
}
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IDataHandler
{
    public TypedValue AddVariable(ref IScope scope, string name, string id, ExpressionValue value);
    public TypedValue SetVariable(ref IScope scope, ScratchScriptVariable variable, ExpressionValue value);
    public TypedValue GetVariable(ref IScope scope, ScratchScriptVariable variable);
    public string GenerateVariableId(int scopeDepth, string visitorId, string variableName);
}
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IDataHandler
{
    public TypedValue AddVariable(IScope scope, string name, string id, ExpressionValue value);
    public TypedValue SetVariable(IScope scope, ScratchScriptVariable variable, ExpressionValue value);
    public TypedValue GetVariable(IScope scope, ScratchScriptVariable variable);
    public string GenerateVariableId(int scopeDepth, string visitorId, string variableName);
}
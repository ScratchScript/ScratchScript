using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IDataHandler
{
    public void AddVariable(ref Scope scope, string name, string id, ExpressionValue value);
    public void SetVariable(ref Scope scope, ScratchScriptVariable variable, ExpressionValue value);
    public TypedValue GetVariable(ref Scope scope, ScratchScriptVariable variable);
    public string GenerateVariableId(int scopeDepth, string visitorId, string variableName);
}
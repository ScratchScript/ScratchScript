using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3DataHandler : IDataHandler
{
    public void AddVariable(ref Scope scope, string name, string id, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        if (!string.IsNullOrEmpty(value.Dependencies)) scope.Content.Add(value.Dependencies);
        scope.Content.Add(BackendHelper.Push(BackendHelper.VariableNamesList, id.Surround('"')));
        scope.Content.Add(BackendHelper.Push(BackendHelper.VariableValuesList, value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) scope.Content.Add(value.Cleanup);

        scope.Variables[name] = new ScratchScriptVariable(name, id, value.Type);
    }

    public void SetVariable(ref Scope scope, ScratchScriptVariable variable, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        if (!string.IsNullOrEmpty(value.Dependencies)) scope.Content.Add(value.Dependencies);
        scope.Content.Add(BackendHelper.SetVariableValue(variable.Id, value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) scope.Content.Add(value.Cleanup);
    }

    public TypedValue GetVariable(ref Scope scope, ScratchScriptVariable variable)
    {
        return new TypedValue(BackendHelper.GetVariableValue(variable.Id), variable.Type);
    }

    public string GenerateVariableId(int scopeDepth, string visitorId, string variableName)
    {
        return $"_{visitorId[..5]}_{scopeDepth}_{variableName}";
    }
}
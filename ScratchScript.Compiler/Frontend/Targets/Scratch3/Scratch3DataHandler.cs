using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3DataHandler : IDataHandler
{
    public void AddVariable(ref IScope scope, string name, string id, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        if (!string.IsNullOrEmpty(value.Dependencies)) scope.Content.Add(value.Dependencies);
        scope.Content.Add(Scratch3Helper.Push(Scratch3Helper.VariableNamesList, id.Surround('"')));
        scope.Content.Add(Scratch3Helper.Push(Scratch3Helper.VariableValuesList, value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) scope.Content.Add(value.Cleanup);

        scope.Variables[name] = new ScratchScriptVariable(name, id, value.Type);
    }

    public void SetVariable(ref IScope scope, ScratchScriptVariable variable, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        if (!string.IsNullOrEmpty(value.Dependencies)) scope.Content.Add(value.Dependencies);
        scope.Content.Add(Scratch3Helper.SetVariableValue(variable.Id, value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) scope.Content.Add(value.Cleanup);
    }

    public TypedValue GetVariable(ref IScope scope, ScratchScriptVariable variable)
    {
        return new TypedValue(Scratch3Helper.GetVariableValue(variable.Id), variable.Type);
    }

    public string GenerateVariableId(int scopeDepth, string visitorId, string variableName)
    {
        return $"_{visitorId[..5]}_{scopeDepth}_{variableName}";
    }
}
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3DataHandler : IDataHandler
{
    public TypedValue AddVariable(IScope scope, string name, string id, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        var commands = new List<string>
        {
            Scratch3Helper.Push(Scratch3Helper.VariableNamesList, id.Surround('"')),
            Scratch3Helper.Push(Scratch3Helper.VariableValuesList, value.Value)
        };

        scope.Variables[name] = new ScratchScriptVariable(name, id, value.Type, value);
        return new StatementValue(commands, value.Dependencies, value.Cleanup);
    }

    public TypedValue SetVariable(IScope scope, ScratchScriptVariable variable, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        var ownerScope = scope.GetVariableOwnerScope(variable.Name);
        if (ownerScope != null) ownerScope.Variables[variable.Name] = variable with { LastKnownValue = value };

        return new StatementValue([Scratch3Helper.SetVariableValue(variable.Id, value.Value)], value.Dependencies,
            value.Cleanup);
    }

    public TypedValue GetVariable(IScope scope, ScratchScriptVariable variable)
    {
        return new ExpressionValue(Scratch3Helper.GetVariableValue(variable.Id), variable.Type);
    }

    public string GenerateVariableId(int scopeDepth, string visitorId, string variableName)
    {
        return $"_{visitorId[..5]}_{scopeDepth}_{variableName}";
    }
}
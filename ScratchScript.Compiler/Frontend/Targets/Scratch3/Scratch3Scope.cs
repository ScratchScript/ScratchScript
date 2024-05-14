using System.Text;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3Scope: Scope
{
    public override string ToString(char separator)
    {
        var sb = new StringBuilder();

        sb.Append(Header);
        sb.Append(separator);

        foreach (var line in Content)
        {
            sb.Append(line);
            sb.Append(separator);
        }

        foreach (var variable in Variables.Values)
        {
            var index = BackendHelper.IndexOf(BackendHelper.VariableNamesList, variable.Id.Surround('"'));

            sb.Append(BackendHelper.PopAt(BackendHelper.VariableNamesList, index));
            sb.Append(separator);
            sb.Append(BackendHelper.PopAt(BackendHelper.VariableValuesList, index));
            sb.Append(separator);
        }

        sb.AppendLine("end");
        return sb.ToString();
    }

    public void AddVariable(string name, string id, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        if (!string.IsNullOrEmpty(value.Dependencies)) Content.Add(value.Dependencies);
        Content.Add(BackendHelper.Push(BackendHelper.VariableNamesList, id.Surround('"')));
        Content.Add(BackendHelper.Push(BackendHelper.VariableValuesList, value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) Content.Add(value.Cleanup);

        Variables[name] = new ScratchScriptVariable(name, id, value.Type);
    }

    public void SetVariable(ScratchScriptVariable variable, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");
        
        if (!string.IsNullOrEmpty(value.Dependencies)) Content.Add(value.Dependencies);
        Content.Add(BackendHelper.SetVariableValue(variable.Id.Surround('"'), value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) Content.Add(value.Cleanup);
    }
}
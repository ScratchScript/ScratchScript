using System.Text;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3Scope : Scope
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
            var index = Scratch3Helper.IndexOf(Scratch3Helper.VariableNamesList, variable.Id.Surround('"'));

            sb.Append(Scratch3Helper.PopAt(Scratch3Helper.VariableNamesList, index));
            sb.Append(separator);
            sb.Append(Scratch3Helper.PopAt(Scratch3Helper.VariableValuesList, index));
            sb.Append(separator);
        }

        sb.AppendLine("end");
        return sb.ToString();
    }

    public void AddVariable(string name, string id, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        if (!string.IsNullOrEmpty(value.Dependencies)) Content.Add(value.Dependencies);
        Content.Add(Scratch3Helper.Push(Scratch3Helper.VariableNamesList, id.Surround('"')));
        Content.Add(Scratch3Helper.Push(Scratch3Helper.VariableValuesList, value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) Content.Add(value.Cleanup);

        Variables[name] = new ScratchScriptVariable(name, id, value.Type);
    }

    public void SetVariable(ScratchScriptVariable variable, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("Cannot set variable to null.");

        if (!string.IsNullOrEmpty(value.Dependencies)) Content.Add(value.Dependencies);
        Content.Add(Scratch3Helper.SetVariableValue(variable.Id.Surround('"'), value.Value));
        if (!string.IsNullOrEmpty(value.Cleanup)) Content.Add(value.Cleanup);
    }
}
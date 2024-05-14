using System.Text;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Information;

public record ScratchScriptVariable(string Name, string Id, ScratchType Type);

public class Scope
{
    public Dictionary<string, ScratchScriptVariable> Variables { get; } = [];
    public int Depth;
    public Scope? ParentScope;

    public readonly List<string> Content = [];
    public string Header = "";

    public string ToString(char separator)
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

    public ScratchScriptVariable? GetVariable(string name)
    {
        var scope = this;
        do
        {
            if (Variables.TryGetValue(name, out var variable)) return variable;
            scope = scope.ParentScope;
        } while (scope?.ParentScope != null);

        return null;
    }

    public int? GetVariableDepth(string name)
    {
        var scope = this;
        do
        {
            if (Variables.ContainsKey(name)) return scope.Depth;
            scope = scope.ParentScope;
        } while (scope?.ParentScope != null);

        return null;
    }
}
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public record ScratchScriptVariable(string Name, string Id, ScratchType Type);

public interface IScope
{
    public List<string> Content { get; init; }
    public int Depth { get; set; }
    public string Header { get; set; }
    public IScope? ParentScope { get; set; }
    public Dictionary<string, ScratchScriptVariable> Variables { get; init; }

    public string ToString(char separator);

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

public interface IFunctionScope : IScope
{
    // dictionaries are not guaranteed to be ordered, so a list is used here
    public List<ScratchScriptVariable> Arguments { get; init; }
    public string FunctionName { get; set; }
    public ScratchType ReturnType { get; set; }
}
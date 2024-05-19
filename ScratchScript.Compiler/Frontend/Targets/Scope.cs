using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public record ScratchScriptVariable(string Name, string Id, ScratchType Type);

public abstract class Scope
{
    public readonly List<string> Content = [];
    public int Depth;
    public string Header = "";
    public Scope? ParentScope;
    public Dictionary<string, ScratchScriptVariable> Variables { get; } = [];

    public abstract string ToString(char separator);

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

public abstract class FunctionScope : Scope
{
    // dictionaries are not guaranteed to be ordered, so a list is used here
    public List<ScratchScriptVariable> Arguments { get; } = [];
    public string FunctionName { get; set; }
}
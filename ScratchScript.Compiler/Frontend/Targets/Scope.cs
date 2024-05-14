using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public record ScratchScriptVariable(string Name, string Id, ScratchType Type);
public abstract class Scope
{
    public Dictionary<string, ScratchScriptVariable> Variables { get; } = [];
    public int Depth;
    public Scope? ParentScope;

    public readonly List<string> Content = [];
    public string Header = "";

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
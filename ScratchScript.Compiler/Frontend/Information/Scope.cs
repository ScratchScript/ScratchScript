using ScratchScript.Compiler.Backend.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Information;

public record ScratchScriptVariable(string Name, ScratchType Type, TypedValue? LastKnownValue);

public class Scope
{
    public List<IrCommandNode> Body { get; set; } = [];
    public int Depth { get; set; }
    public Scope? ParentScope { get; set; }
    public Dictionary<string, ScratchScriptVariable> Variables { get; set; } = new();

    public ScratchScriptVariable? GetVariable(string name)
    {
        var scope = this;
        do
        {
            if (scope.Variables.TryGetValue(name, out var variable)) return variable;
            scope = scope.ParentScope;
        } while (scope != null);

        return null;
    }

    public int? GetVariableDepth(string name)
    {
        var scope = this;
        do
        {
            if (scope.Variables.ContainsKey(name)) return scope.Depth;
            scope = scope.ParentScope;
        } while (scope != null);

        return null;
    }

    public Scope? GetVariableOwnerScope(string name)
    {
        var scope = this;
        do
        {
            if (scope.Variables.ContainsKey(name)) return scope;
            scope = scope.ParentScope;
        } while (scope != null);

        return null;
    }

    public virtual Scope CloneWithTransformedBody(Func<IrNode, IrNode> visitor)
    {
        var target = new Scope();
        PopulateClone(target, visitor);
        return target;
    }

    protected void PopulateClone(Scope target, Func<IrNode, IrNode> visitor)
    {
        target.Depth = Depth;
        target.ParentScope = target.ParentScope?.CloneWithTransformedBody(visitor);
        target.Body = Body.Select(n => (IrCommandNode)visitor(n)).ToList();
        target.Variables = new Dictionary<string, ScratchScriptVariable>(Variables);
    }
}

public class FunctionScope : Scope
{
    public string Id { get; set; }

    // dictionaries are not guaranteed to be ordered, so a list is used here
    public List<ScratchScriptVariable> Arguments { get; set; }
    public string FunctionName { get; set; }
    public ScratchType ReturnType { get; set; }

    public string SignatureString =>
        StringExtensions.GetFunctionSignatureString(FunctionName, Arguments.Select(arg => arg.Type));

    public override Scope CloneWithTransformedBody(Func<IrNode, IrNode> visitor)
    {
        var target = new FunctionScope();
        PopulateClone(target, visitor);
        target.Id = Id;
        target.ReturnType = ReturnType;
        target.FunctionName = FunctionName;
        target.Arguments = new List<ScratchScriptVariable>(Arguments);
        return target;
    }
}
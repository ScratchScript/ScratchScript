using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.AST.Information;

public record ScratchScriptVariable
{
    public string Name { get; init; }
    public ScratchType Type { get; set; }

    public ScratchScriptVariable(string name, ScratchType? type = null)
    {
        Name = name;
        Type = type ?? ScratchType.Unknown;
    }
}

public class Scope
{
    public List<IrCommandNode> Body { get; set; } = [];
    public int Depth { get; set; }
    public Scope? ParentScope { get; set; }
    public List<ScratchScriptVariable> Variables { get; set; } = new();

    public ScratchScriptVariable? GetVariable(string name)
    {
        var scope = this;
        do
        {
            var variable = scope.Variables.FirstOrDefault(v => v.Name == name);
            if (variable != null) return variable;
            scope = scope.ParentScope;
        } while (scope != null);

        return null;
    }

    public ScratchScriptVariable? GetArgument(string name)
    {
        var scope = this;
        do
        {
            if (scope is FunctionScope functionScope)
            {
                var argument = functionScope.Arguments.FirstOrDefault(a => a.Name == name);
                if (argument != null) return argument;
            }

            scope = scope.ParentScope;
        } while (scope != null);

        return null;
    }

    public int? GetVariableDepth(string name)
    {
        var scope = this;
        do
        {
            if (scope.Variables.Any(v => v.Name == name)) return scope.Depth;
            scope = scope.ParentScope;
        } while (scope != null);

        return null;
    }

    public Scope? GetVariableOwnerScope(string name)
    {
        var scope = this;
        do
        {
            if (scope.Variables.Any(v => v.Name == name)) return scope;
            scope = scope.ParentScope;
        } while (scope != null);

        return null;
    }

    public FunctionScope? GetClosestFunctionScope()
    {
        var scope = this;
        do
        {
            if (scope is FunctionScope functionScope) return functionScope;
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
        target.Variables = new List<ScratchScriptVariable>(Variables);
    }
}

public class FunctionScope : Scope
{
    public string Id { get; set; }

    // dictionaries are not guaranteed to be ordered, so a list is used here
    public List<ScratchScriptVariable> Arguments { get; set; } = [];
    public string FunctionName { get; set; }
    public ScratchType ReturnType { get; set; }

    public string SignatureString =>
        StringExtensions.GetFunctionSignatureString(FunctionName, Arguments.Select(arg => arg.Type));

    public bool UseArgumentReporters { get; set; }

    public override Scope CloneWithTransformedBody(Func<IrNode, IrNode> visitor)
    {
        var target = new FunctionScope();
        PopulateClone(target, visitor);
        target.Id = Id;
        target.ReturnType = ReturnType;
        target.FunctionName = FunctionName;
        target.Arguments = new List<ScratchScriptVariable>(Arguments);
        target.UseArgumentReporters = UseArgumentReporters;
        return target;
    }
}
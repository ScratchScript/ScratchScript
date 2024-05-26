using System.Text;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3Scope : IScope
{
    // the "stack debt" value is unique to the Scratch3 target, since it
    // doesn't have a native function return value reporter.
    // the concept is as follows: every function call increases this value
    // by one. the compiler should strive to keep this value at zero, i.e.
    // popping the function return value right after the statement in which
    // it was called. this value is used for correctly locating where the
    // function return value be for a specific function call.
    public int StackDebt { get; set; }

    public int TotalStackDebt
    {
        get
        {
            var total = 0;
            var scope = this;
            do
            {
                total += scope.StackDebt;
                scope = scope.ParentScope as Scratch3Scope;
            } while (scope != null);

            return total;
        }
    }

    public List<string> Content { get; init; } = [];
    public int Depth { get; set; } = 0;
    public List<string> Header { get; set; } = [];
    public List<string> End { get; set; } = ["end"];
    public IScope? ParentScope { get; set; } = null;
    public Dictionary<string, ScratchScriptVariable> Variables { get; init; } = [];

    public string ToString(char separator)
    {
        return new DefaultScratch3ScopeFormatter(this, separator).ToString();
    }
}

public class Scratch3FunctionScope : Scratch3Scope, IFunctionScope
{
    public new string ToString(char separator)
    {
        return new DefaultScratch3ScopeFormatter(this, separator).ToString();
    }

    public List<ScratchScriptVariable> Arguments { get; init; } = [];
    public string FunctionName { get; set; } = "";
    public ScratchType ReturnType { get; set; } = ScratchType.Void;
}

internal class DefaultScratch3ScopeFormatter(IScope scope, char separator)
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var line in scope.Header)
        {
            sb.Append(line);
            sb.Append(separator);
        }

        foreach (var line in scope.Content)
        {
            sb.Append(line);
            sb.Append(separator);
        }

        foreach (var variable in scope.Variables.Values)
        {
            var index = Scratch3Helper.IndexOf(Scratch3Helper.VariableNamesList, variable.Id.Surround('"'));

            sb.Append(Scratch3Helper.PopAt(Scratch3Helper.VariableValuesList, index));
            sb.Append(separator);
            sb.Append(Scratch3Helper.PopAt(Scratch3Helper.VariableNamesList, index));
            sb.Append(separator);
        }
        
        sb.AppendJoin(separator, scope.End);
        return sb.ToString();
    }
}
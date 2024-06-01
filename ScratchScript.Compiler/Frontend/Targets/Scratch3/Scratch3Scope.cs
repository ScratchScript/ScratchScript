using System.Text;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3Scope : IScope
{
    // stores the length of the intermediate stack, which is used
    // for ALL function return values and complex calculations.
    // this value is increased by one every function call, and
    // resets to zero at the start of the next line.
    public int IntermediateStackCount { get; set; }

    public int TotalIntermediateStackCount
    {
        get
        {
            var total = 0;
            var scope = this;
            do
            {
                total += scope.IntermediateStackCount;
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
    public bool Inlined { get; set; } = false;
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
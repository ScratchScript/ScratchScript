using System.Text;
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3Scope : Scope
{
    public override string ToString(char separator)
    {
        return new DefaultScratch3ScopeFormatter(this, separator).ToString();
    }
}

public class Scratch3FunctionScope : FunctionScope
{
    public override string ToString(char separator)
    {
        return new DefaultScratch3ScopeFormatter(this, separator).ToString();
    }
}

internal class DefaultScratch3ScopeFormatter(Scope scope, char separator)
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(scope.Header);
        sb.Append(separator);

        foreach (var line in scope.Content)
        {
            sb.Append(line);
            sb.Append(separator);
        }

        foreach (var variable in scope.Variables.Values)
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
}
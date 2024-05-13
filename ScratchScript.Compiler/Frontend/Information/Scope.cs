using System.Text;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Information;

public class Scope
{
    public List<(string Name, ScratchType Type)> Variables { get; } = [];
    public int Depth;
    public Scope? ParentScope;

    public List<string> Content = [];
    public string Header;

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.Append(Header);
        sb.Append(' ');
        
        foreach (var line in Content)
        {
            sb.Append(line);
            sb.Append(' ');
        }

        for (var index = 0; index < Variables.Count; index++)
        {
            sb.Append(BackendHelper.PopStack());
            sb.Append(' ');
        }

        sb.AppendLine("end");
        return sb.ToString();
    }

    public void AddVariable(string name, ExpressionValue value)
    {
        if (value.Value == null) throw new Exception("cannot set variable to null");
        
        Content.Add(value.Dependencies);
        Content.Add(BackendHelper.PushToStack(value.Value));
        Content.Add(value.Cleanup);
        
        Variables.Add((name, value.Type));
    }
}
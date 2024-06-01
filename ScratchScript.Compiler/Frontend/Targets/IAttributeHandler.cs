using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IAttributeHandler
{
    public List<string> TopLevelAttributes { get; protected init; }
    public List<string> FunctionAttributes { get; protected init; }

    public void ProcessTopLevelAttribute(string attributeName, IEnumerable<TypedValue> values);
    public void ProcessFunctionAttribute(IScope scope, string attributeName, IEnumerable<TypedValue> values);
}
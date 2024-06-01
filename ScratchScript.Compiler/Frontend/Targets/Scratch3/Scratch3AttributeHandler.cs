using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3AttributeHandler : IAttributeHandler
{
    public List<string> TopLevelAttributes { get; init; } =
    [
    ];

    public List<string> FunctionAttributes { get; init; } =
    [
        "inline()"
    ];

    public void ProcessTopLevelAttribute(string attributeName, IEnumerable<TypedValue> values)
    {
        switch (attributeName)
        {
        }
    }

    public void ProcessFunctionAttribute(IScope scope, string attributeName, IEnumerable<TypedValue> values)
    {
        switch (attributeName)
        {
            case "inline":
            {
                if (scope is IFunctionScope functionScope) functionScope.Inlined = true;
                break;
            }
        }
    }
}
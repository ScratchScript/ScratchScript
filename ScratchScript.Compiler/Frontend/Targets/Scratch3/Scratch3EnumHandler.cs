using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public class Scratch3EnumHandler : IEnumHandler
{
    public const string EnumIdsList = "__EID";
    public const string EnumPropertiesList = "__EP";
    public const string EnumValuesList = "__EV";

    public IEnumerable<string> ConvertEnumsToBackend(IEnumerable<EnumScratchType> typesEnumerable)
    {
        var types = typesEnumerable.ToList();
        if (types.Count == 0) return [];

        var values = types
            .Select(type => type.Values.Values.Select(value =>
                value.Value)).SelectMany(list => list);
        var properties = types.Select(type => type.Values.Keys.Select(name => name.Surround('"')))
            .SelectMany(list => list);
        var ids = types.Select(type => type.Values.Keys.Select(name => $"\"{type.Name}_{name}\""))
            .SelectMany(list => list);

        return
        [
            $"define list {EnumIdsList} {string.Join(' ', ids)}",
            $"define list {EnumPropertiesList} {string.Join(' ', properties)}",
            $"define list {EnumValuesList} {string.Join(' ', values)}"
        ];
    }

    public ExpressionValue GetEnumValue(EnumScratchType type, TypedValue id)
    {
        if (id is ExpressionValue idExpression)
            return new ExpressionValue(
                Scratch3Helper.ItemOf(EnumValuesList, Scratch3Helper.IndexOf(EnumIdsList, id.Value!)),
                type.ChildType ?? throw new Exception("Cannot get a value from an enum of unknown type."),
                idExpression.Dependencies, idExpression.Cleanup);

        return new ExpressionValue(
            Scratch3Helper.ItemOf(EnumValuesList, Scratch3Helper.IndexOf(EnumIdsList, id.Value!)),
            type.ChildType ?? throw new Exception("Cannot get a value from an enum of unknown type."));
    }

    public ExpressionValue GetEnumName(EnumScratchType type, TypedValue id)
    {
        if (id is ExpressionValue idExpression)
            return new ExpressionValue(
                Scratch3Helper.ItemOf(EnumPropertiesList, Scratch3Helper.IndexOf(EnumIdsList, id.Value!)),
                type.ChildType ?? throw new Exception("Cannot get a value from an enum of unknown type."),
                idExpression.Dependencies, idExpression.Cleanup);

        return new ExpressionValue(
            Scratch3Helper.ItemOf(EnumPropertiesList, Scratch3Helper.IndexOf(EnumIdsList, id.Value!)),
            type.ChildType ?? throw new Exception("Cannot get a value from an enum of unknown type."));
    }
}
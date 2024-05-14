using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.Frontend;

public static class BackendHelper
{
    public const string StackList = "__Stack";
    public const string VariableNamesList = "__VN";
    public const string VariableValuesList = "__VV";

    public static string Push(string list, object value) => $"push {list} {value}";
    public static string Pop(string list) => $"pop {list}";
    public static string PopAt(string list, string index) => $"popat {list} {index}";

    public static string IndexOf(string list, object value) =>
        $"rawshadow data_itemnumoflist f:LIST:{list.Surround('"')} i:ITEM:{value} endshadow";

    public static string ItemOf(string list, string index) =>
        $"rawshadow data_itemoflist f:LIST:{list.Surround('"')} i:INDEX:{index} endshadow";

    public static string Replace(string list, string index, object item) =>
        $"raw data_replaceitemoflist f:LIST:{list.Surround('"')} i:INDEX:{index} i:ITEM:{item}";

    public static string GetVariableValue(string id) =>
        ItemOf(VariableValuesList, IndexOf(VariableNamesList, id.Surround('"')));

    public static string SetVariableValue(string id, object value) =>
        Replace(VariableValuesList, IndexOf(VariableNamesList, id.Surround('"')), value);
}
using ScratchScript.Compiler.Extensions;

namespace ScratchScript.Compiler.Frontend.Targets.Scratch3;

public static class Scratch3Helper
{
    public const string StackList = "__Stack";
    public const string VariableNamesList = "__VN";
    public const string VariableValuesList = "__VV";

    public static string CallFunction(string name)
    {
        return $"call {name}";
    }

    public static string Push(string list, object value)
    {
        return $"push {list} {value}";
    }

    public static string PushAt(string list, string index, object value)
    {
        return $"pushat {list} {index} {value}";
    }

    public static string Pop(string list)
    {
        return $"pop {list}";
    }

    public static string PopAt(string list, string index)
    {
        return $"popat {list} {index}";
    }

    public static string Repeat(string times, string action)
    {
        return $"repeat {times} {action} end";
    }

    public static string IndexOf(string list, object value)
    {
        return $"rawshadow data_itemnumoflist f:LIST:{list.Surround('"')} i:ITEM:{value} endshadow";
    }

    public static string ItemOf(string list, string index)
    {
        return $"rawshadow data_itemoflist f:LIST:{list.Surround('"')} i:INDEX:{index} endshadow";
    }

    public static string Replace(string list, string index, object item)
    {
        return $"raw data_replaceitemoflist f:LIST:{list.Surround('"')} i:INDEX:{index} i:ITEM:{item}";
    }

    public static string GetVariableValue(string id)
    {
        return ItemOf(VariableValuesList, IndexOf(VariableNamesList, id.Surround('"')));
    }

    public static string SetVariableValue(string id, object value)
    {
        return Replace(VariableValuesList, IndexOf(VariableNamesList, id.Surround('"')), value);
    }

    public static string StopThisScript()
    {
        return "raw control_stop f:STOP_OPTION:\"this script\"";
    }
}
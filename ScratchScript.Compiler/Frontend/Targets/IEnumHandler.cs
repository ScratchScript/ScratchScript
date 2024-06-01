using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Targets;

public interface IEnumHandler
{
    public IEnumerable<string> ConvertEnumsToBackend(IEnumerable<EnumScratchType> types);
    public ExpressionValue GetEnumValue(EnumScratchType type, TypedValue id);
    public ExpressionValue GetEnumName(EnumScratchType type, TypedValue id);
}
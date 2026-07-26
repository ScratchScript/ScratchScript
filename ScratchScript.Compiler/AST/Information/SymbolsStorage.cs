using ScratchScript.Compiler.AST.Representation;

namespace ScratchScript.Compiler.AST.Information;

public class ImportedNodesStorage
{
    public Dictionary<string, ImportedNode<IrFunctionNode>> Functions = [];
    public Dictionary<string, ImportedNode<IrExpressionNode>> Globals = [];
}

public class SymbolsStorage
{
    public Dictionary<string, SymbolsNamespace> Namespaces { get; } = [];

    public SymbolsNamespace this[string ns]
    {
        get
        {
            if (!Namespaces.ContainsKey(ns)) Namespaces[ns] = new SymbolsNamespace();
            return Namespaces[ns];
        }
    }
}

public class SymbolsNamespace
{
    public List<string> Enums { get; } = [];
    public List<string> Globals { get; } = [];
    public Dictionary<string, List<ImportSymbol>> Functions { get; } = [];
}

public record ImportSymbol(string Namespace, string Member, string? ImportAs);

public record ImportedNode<T>(string Namespace, string OriginalName, T Node);
using MessagePack;
using MessagePack.Resolvers;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.AST.Information;

public class ImportedNodesStorage
{
    public Dictionary<string, ImportedNode<IrFunctionNode>> Functions = [];
    public Dictionary<string, ImportedNode<IrExpressionNode>> Globals = [];
}

public record ImportedNode<T>(string Namespace, string OriginalName, T Node);

[MessagePackObject]
public class SymbolsStorage
{
    [Key(0)] public Dictionary<string, SymbolsNamespace> Namespaces = [];

    [IgnoreMember]
    public SymbolsNamespace this[string ns]
    {
        get
        {
            if (!Namespaces.ContainsKey(ns)) Namespaces[ns] = new SymbolsNamespace();
            return Namespaces[ns];
        }
    }
}

[MessagePackObject]
public class SymbolsNamespace
{
    [Key(0)] public Dictionary<string, SymbolsArtifact> Artifacts = [];

    [IgnoreMember]
    public SymbolsArtifact this[string filename]
    {
        get
        {
            if (!Artifacts.ContainsKey(filename)) Artifacts[filename] = new SymbolsArtifact();
            return Artifacts[filename];
        }
    }

    public string CreateArtifact(string path)
    {
        var relativePath = Path.GetRelativePath("src", path);
        var targetRelativePath = Path.ChangeExtension(relativePath, ".cscrs");
        var key = Path.Combine("out", targetRelativePath);
        Artifacts[key] = new SymbolsArtifact { Source = path };
        return key;
    }
}

[MessagePackObject]
public class SymbolsArtifact
{
    [Key(1)] public List<string> Enums = [];
    [Key(3)] public Dictionary<string, List<ImportSymbol>> Functions = [];
    [Key(2)] public List<string> Globals = [];
    [Key(0)] public string Source = null!;
}

[MessagePackObject]
public record ImportSymbol(
    [property: Key(0)] string Namespace,
    [property: Key(1)] string Member,
    [property: Key(2)] string? ImportAs);

public static class SymbolsStorageSerializer
{
    private static readonly MessagePackSerializerOptions Options;

    static SymbolsStorageSerializer() =>
        Options = MessagePackSerializerOptions.Standard.WithResolver(
                CompositeResolver.Create([ScratchTypeFormatter.Instance],
                    [StandardResolver.Instance]))
            .WithCompression(MessagePackCompression.Lz4BlockArray);

    public static byte[] Serialize(SymbolsStorage root) => MessagePackSerializer.Serialize(root, Options);

    public static SymbolsStorage Deserialize(byte[] bytes) =>
        MessagePackSerializer.Deserialize<SymbolsStorage>(bytes, Options);
}
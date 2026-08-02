using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;

namespace ScratchScript.Compiler.ImportsHandling;

// TODO: very crude implementation
public class ScratchScriptImportsHandler(SymbolsStorage Symbols) : IrRewriter
{
    public bool Success { get; private set; } = true;
    public ImportedNodesStorage ImportedNodes { get; } = new();

    private bool TryImport(IrImportNode node)
    {
        if (TryCandidate(Directory.GetCurrentDirectory(), node, Symbols)) return true;

        var directoryName = node.From.Split("/").FirstOrDefault();
        if (string.IsNullOrEmpty(directoryName)) return false;

        var baseDirectoryCandidates = new List<string>([Directory.GetCurrentDirectory(), AppContext.BaseDirectory]);
        var found = false;
        foreach (var candidate in baseDirectoryCandidates)
        {
            var targetDirectory = Path.Join(candidate, directoryName);
            if (!Directory.Exists(targetDirectory)) continue;

            var symbolsFile = Directory.EnumerateFiles(targetDirectory, "*.symbols", SearchOption.AllDirectories)
                .ToList();
            if (symbolsFile.Count == 0) continue;
            if (symbolsFile.Count > 1) return false;

            var symbols = SymbolsStorageSerializer.Deserialize(File.ReadAllBytes(symbolsFile.First()));
            if (TryCandidate(targetDirectory, node, symbols))
            {
                found = true;
                break;
            }
        }

        if (!found) return false;
        return true;
    }

    public override IrNode VisitImport(IrImportNode node)
    {
        Success = TryImport(node);
        return node;
    }

    private bool TryCandidate(string basePath, IrImportNode node, SymbolsStorage storage)
    {
        if (!storage.Namespaces.TryGetValue(node.From, out var ns)) return false;

        foreach (var (member, importAs) in node.Members)
        {
            var (path, artifact) = ns.Artifacts.FirstOrDefault(kvp =>
                kvp.Value.Functions.ContainsKey(member) || kvp.Value.Globals.Contains(member) ||
                kvp.Value.Enums.Contains(member));
            if (artifact is null) return false;

            var importedNode = IrTreeSerializer.Deserialize(File.ReadAllBytes(Path.Join(basePath, path)));
            if (artifact.Functions.TryGetValue(member, out var function))
            {
                if (function.Any(dependency => !TryImport(new IrImportNode(dependency.Namespace,
                        new Dictionary<string, string?> { { dependency.Member, dependency.ImportAs } }))))
                    return false;
                var importedFunction =
                    importedNode.Functions.FirstOrDefault(f => f.FunctionScope.FunctionName == member);
                if (importedFunction is null) return false;

                ImportedNodes.Functions[importAs ?? member] =
                    new ImportedNode<IrFunctionNode>(node.From, member, importedFunction);
            }
        }

        return true;
    }

    public override IrNode VisitProgram(IrProgramNode node)
    {
        var program = (IrProgramNode)base.VisitProgram(node);
        return program with
        {
            TopLevelNodes = program.TopLevelNodes.Concat(ImportedNodes.Functions.Values.Select(ifu => ifu.Node))
                .ToList(),
            Defines = program.Defines.Concat(ImportedNodes.Globals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Node))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}
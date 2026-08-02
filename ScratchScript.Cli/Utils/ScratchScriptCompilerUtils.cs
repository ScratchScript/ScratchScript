using Antlr4.Runtime;
using ScratchScript.Compiler.AST.Builder;
using ScratchScript.Compiler.AST.GeneratedVisitor;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.AST.Representation;
using ScratchScript.Compiler.Extensions;
using ScratchScript.Compiler.ImportsHandling;
using ScratchScript.Compiler.ProjectEmitter;
using ScratchScript.Compiler.ProjectEmitter.Helpers;
using ScratchScript.Compiler.ProjectEmitter.Models;
using ScratchScript.Compiler.Rewriters.Codegen.HighLevel;
using ScratchScript.Compiler.Rewriters.Codegen.LowLevel;
using ScratchScript.Compiler.Rewriters.TargetLowering;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Cli.Utils;

public static class ScratchScriptCompilerUtils
{
    private static readonly Dictionary<CodegenLevel, List<IrRewriter>> Rewriters =
        new()
        {
            {
                CodegenLevel.High, [
                    new RawFunctionsExpansionRewriter(),
                    new ControlFlowDesugarizationRewriter(),
                    new FunctionInlineRewriter(),
                    new CompilerFunctionsExpansionRewriter()
                ]
            },
            { CodegenLevel.LoweringPass, [new Scratch3LoweringPass()] },
            {
                CodegenLevel.Low, [
                    new ComplexExpressionUnwindingRewriter(),
                    new LoopSynthesisRewriter(),
                    new OperatorUnwindingRewriter(),
                    new UnusedFunctionsRemovalRewriter()
                ]
            }
        };

    public static (ScratchScriptVisitor, IrProgramNode?) BuildAst(SymbolsStorage symbols, string sourcePath)
    {
        var inputStream = new AntlrFileStream(sourcePath);
        var lexer = new ScratchScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new ScratchScriptParser(tokenStream);
        var visitor = new ScratchScriptVisitor(symbols, sourcePath);
        var result = (IrProgramNode?)visitor.Visit(parser.program());
        return (visitor, result);
    }

    public static (IrProgramNode, bool) TypeCheck(IrProgramNode node)
    {
        var typeChecker = new ScratchScriptTypeChecker();
        node = (IrProgramNode)typeChecker.VisitProgram(node);
        return (node, typeChecker.Success);
    }

    public static Target EmitTarget(IrProgramNode node, string sourcePath)
    {
        var emitter = new ScratchScriptProjectEmitter(EnumerableExtensions.ToMd5Checksum(sourcePath));
        emitter.VisitProgram(node);
        var target = emitter.Target;
        target.LayerOrder = 1; // TODO: make this an attribute
        target.Name = Path.GetFileNameWithoutExtension(sourcePath);
        target.Costumes.Add(CostumeHelper.GetEmptyCostume());
        return emitter.Target;
    }

    public static (IrProgramNode, bool) HandleImports(IrProgramNode node, SymbolsStorage symbols)
    {
        var importer = new ScratchScriptImportsHandler(symbols);
        node = (IrProgramNode)importer.VisitProgram(node);
        return (node, importer.Success);
    }

    public static IrProgramNode RunCodegen(CodegenLevel level, IrProgramNode node, Action<string> log)
    {
        foreach (var rewriter in Rewriters[level])
            node = RewriteBenchmarked(rewriter);

        return node;

        IrProgramNode RewriteBenchmarked(IrRewriter rewriter)
        {
            var (newNode, time) = Benchmarker.Measure(() => IrRewriterUtils.RewriteUntilNoChanges(rewriter, node));
            log($"{rewriter.GetType().Name} ({time}ms)");
            return newNode;
        }
    }
}

public enum CodegenLevel
{
    High,
    LoweringPass,
    Low
}
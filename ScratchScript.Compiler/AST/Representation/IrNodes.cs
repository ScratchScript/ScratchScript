using System.Collections.Immutable;
using Antlr4.Runtime;
using ScratchScript.Compiler.AST.Information;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.AST.Representation;

public enum IrBinaryOperator
{
    // math
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Power,

    // bitwise
    BitwiseOr,
    BitwiseAnd,
    BitwiseXor,
    BitwiseLeftShift,
    BitwiseRightShift,

    // strings
    Join,

    // boolean
    And,
    Or,
    Xor,
    
    Equal,
    NotEqual,
    
    LessThan,
    LessOrEqualTo,
    GreaterThan,
    GreaterOrEqualTo
}

public enum IrUnaryOperator
{
    Plus,
    Minus,
    Not
}

public abstract record IrNode
{
    public ParserRuleContext Context { get; init; } = ParserRuleContext.EMPTY;
    public ImmutableHashSet<string> Flags = ImmutableHashSet<string>.Empty;
}

public interface ITargetSpecificNode
{
    int GetNodeHash();
    IrNode Rewrite(Func<IrNode, IrNode> visit);
}

public abstract record IrCommandNode : IrNode;

public abstract record IrAttributeNode(string Name, IEnumerable<IrExpressionNode> Arguments) : IrNode;

public abstract record IrExpressionNode : IrNode
{
    public ScratchType InferredType { get; set; } = ScratchType.Unknown;
}

// blocks
public record IrBlockNode(Scope Scope) : IrNode
{
    public IrBlockNode(IEnumerable<IrCommandNode> commands) : this(new Scope { Body = commands.ToList() })
    {
    }
}

public record IrProgramNode(
    IEnumerable<IrFunctionNode> Functions,
    IEnumerable<IrEventNode> Events,
    Dictionary<string, TypedValue> Defines) : IrNode;

public record IrFunctionNode(bool Warp, FunctionScope FunctionScope, IEnumerable<IrAttributeNode>? Attributes = null)
    : IrBlockNode(FunctionScope);

public record IrEventNode(string Type, Scope Scope) : IrBlockNode(Scope);

// expressions
public record IrConstantExpressionNode(TypedValue Value) : IrExpressionNode;

public record IrGlobalVariableIdentifierExpressionNode(string Name) : IrExpressionNode;

public record IrGlobalListIdentifierExpressionNode(string Name) : IrExpressionNode;

public record IrLocalVariableIdentifierExpressionNode(string Name) : IrExpressionNode;

public record IrParenthesizedExpressionNode(IrExpressionNode Expression) : IrExpressionNode;

public record IrBinaryExpressionNode(IrBinaryOperator Operator, IrExpressionNode Left, IrExpressionNode Right)
    : IrExpressionNode;

public record IrUnaryExpressionNode(IrUnaryOperator Operator, IrExpressionNode Operand) : IrExpressionNode;

public record IrShadowExpressionNode(
    string Opcode,
    Dictionary<string, IrExpressionNode> Inputs,
    Dictionary<string, IrExpressionNode> Fields,
    ScratchType? ExpectedType = null) : IrExpressionNode;

public record IrFunctionArgumentExpressionNode(string Name) : IrExpressionNode;

public record IrFunctionCallExpressionNode(string Function, IEnumerable<IrExpressionNode> Arguments)
    : IrExpressionNode;

// mostly for targets which require inserting additional commands before/after the statement (e.g. scratch itself)
public record IrComplexExpressionNode(
    IrExpressionNode Expression,
    IrCommandNode? Dependencies = null,
    IrCommandNode? Cleanup = null)
    : IrExpressionNode;

// mostly scratch3 specific
public record IrStackPointerExpressionNode(int Offset) : IrExpressionNode;

public record IrObjectLiteralExpressionNode(Dictionary<string, IrExpressionNode> Values) : IrExpressionNode;

public record IrTernaryExpressionNode(
    IrExpressionNode Condition,
    IrExpressionNode TrueValue,
    IrExpressionNode FalseValue) : IrExpressionNode;

// commands
public record IrNoOpCommandNode : IrCommandNode;

public record IrCommandSequenceNode(IEnumerable<IrCommandNode> Commands) : IrCommandNode;

public record IrSetCommandNode(string Variable, IrExpressionNode Expression) : IrCommandNode;

public record IrCallFunctionCommandNode(string Function, IEnumerable<IrExpressionNode> Arguments)
    : IrCommandNode;

public record IrRawCommandNode(
    string Opcode,
    Dictionary<string, IrExpressionNode> Inputs,
    Dictionary<string, IrExpressionNode> Fields) : IrCommandNode;

// lists
public record IrPushCommand(string List, IrExpressionNode Expression) : IrCommandNode;

public record IrPushAtCommand(string List, IrExpressionNode Where, IrExpressionNode Expression) : IrCommandNode;

public record IrPopCommand(string List) : IrCommandNode;

public record IrPopAtCommand(string List, IrExpressionNode Where) : IrCommandNode;

public record IrPopAllCommand(string List) : IrCommandNode;

// control flow

public record IrIfCommandNode(
    IrExpressionNode Condition,
    IrBlockNode Body,
    IrBlockNode? Alternate) : IrCommandNode;

public record IrWhileCommandNode(IrExpressionNode Condition, IrBlockNode Body) : IrCommandNode;

public record IrRepeatCommandNode(IrExpressionNode Times, IrBlockNode Body) : IrCommandNode;

public record IrBreakCommandNode : IrCommandNode;

public record IrContinueCommandNode : IrCommandNode;

public record IrReturnCommandNode(IrExpressionNode? ReturnValue) : IrCommandNode;

public static class IrHasher
{
    public static int GetNodeHash(IrNode? node)
        => node switch
        {
            null => 0,
            ITargetSpecificNode targetSpecific => targetSpecific.GetNodeHash(),

            // blocks
            IrProgramNode program => HashCode.Combine(1, HashEnumerable(program.Functions.Select(GetNodeHash)),
                HashEnumerable(program.Events.Select(GetNodeHash))),
            IrFunctionNode func => HashCode.Combine(2, func.Warp, func.FunctionScope.FunctionName.GetHashCode(),
                HashEnumerable(func.FunctionScope.Body.Select(GetNodeHash))),
            IrEventNode ev => HashCode.Combine(3, ev.Type.GetHashCode(),
                HashEnumerable(ev.Scope.Body.Select(GetNodeHash))),
            IrBlockNode block => HashCode.Combine(4, HashEnumerable(block.Scope.Body.Select(GetNodeHash))),

            // expressions
            IrConstantExpressionNode constant => HashCode.Combine(5, constant.Value.GetHashCode()),
            IrLocalVariableIdentifierExpressionNode local => HashCode.Combine(6, local.Name.GetHashCode()),
            IrGlobalVariableIdentifierExpressionNode globalVar => HashCode.Combine(7,
                globalVar.Name.GetHashCode()),
            IrGlobalListIdentifierExpressionNode globalList => HashCode.Combine(8, globalList.Name.GetHashCode()),
            IrFunctionArgumentExpressionNode arg => HashCode.Combine(9, arg.Name.GetHashCode()),
            IrParenthesizedExpressionNode paren => HashCode.Combine(10, GetNodeHash(paren.Expression)),
            IrBinaryExpressionNode bin => HashCode.Combine(11, bin.Operator, GetNodeHash(bin.Left),
                GetNodeHash(bin.Right)),
            IrUnaryExpressionNode unary => HashCode.Combine(12, unary.Operator, GetNodeHash(unary.Operand)),
            IrFunctionCallExpressionNode callExpr =>
                HashCode.Combine(13, callExpr.Function.GetHashCode(),
                    HashEnumerable(callExpr.Arguments.Select(GetNodeHash))),
            IrShadowExpressionNode shadow =>
                HashCode.Combine(14, shadow.Opcode.GetHashCode(), shadow.ExpectedType,
                    HashExpressionDictionary(shadow.Inputs),
                    HashExpressionDictionary(shadow.Fields)),
            IrComplexExpressionNode complex =>
                HashCode.Combine(15, GetNodeHash(complex.Expression), GetNodeHash(complex.Dependencies),
                    GetNodeHash(complex.Cleanup)),
            IrObjectLiteralExpressionNode obj =>
                HashCode.Combine(16, HashExpressionDictionary(obj.Values)),
            IrStackPointerExpressionNode stp => HashCode.Combine(17, stp.Offset.GetHashCode()),
            IrTernaryExpressionNode ternary => HashCode.Combine(18, GetNodeHash(ternary.Condition),
                GetNodeHash(ternary.TrueValue), GetNodeHash(ternary.FalseValue)),

            // commands
            IrNoOpCommandNode => HashCode.Combine(19),
            IrCommandSequenceNode seq => HashCode.Combine(20, HashEnumerable(seq.Commands.Select(GetNodeHash))),
            IrSetCommandNode set => HashCode.Combine(21, set.Variable.GetHashCode(),
                GetNodeHash(set.Expression)),
            IrCallFunctionCommandNode callCmd => HashCode.Combine(22, callCmd.Function.GetHashCode(),
                HashEnumerable(callCmd.Arguments.Select(GetNodeHash))),
            IrReturnCommandNode ret => HashCode.Combine(23, GetNodeHash(ret.ReturnValue)),
            IrRawCommandNode raw =>
                HashCode.Combine(24, raw.Opcode.GetHashCode(), HashExpressionDictionary(raw.Inputs),
                    HashExpressionDictionary(raw.Fields)),

            // lists
            IrPushCommand push => HashCode.Combine(25, push.List.GetHashCode(), GetNodeHash(push.Expression)),
            IrPushAtCommand pushAt => HashCode.Combine(26, pushAt.List.GetHashCode(), GetNodeHash(pushAt.Where),
                GetNodeHash(pushAt.Expression)),
            IrPopCommand pop => HashCode.Combine(27, pop.List.GetHashCode()),
            IrPopAtCommand popAt => HashCode.Combine(28, popAt.List.GetHashCode(), GetNodeHash(popAt.Where)),
            IrPopAllCommand popAll => HashCode.Combine(29, popAll.List.GetHashCode()),

            // control Flow
            IrIfCommandNode cond => HashCode.Combine(30, GetNodeHash(cond.Condition), GetNodeHash(cond.Body),
                GetNodeHash(cond.Alternate)),
            IrWhileCommandNode loop =>
                HashCode.Combine(31, GetNodeHash(loop.Condition), GetNodeHash(loop.Body)),
            IrRepeatCommandNode repeat => HashCode.Combine(32, GetNodeHash(repeat.Times),
                GetNodeHash(repeat.Body)),
            IrBreakCommandNode => HashCode.Combine(33),
            IrContinueCommandNode => HashCode.Combine(34),

            _ => throw new ArgumentOutOfRangeException(nameof(node), $"Unhandled node type: {node.GetType().Name}")
        };

    private static int HashEnumerable(IEnumerable<int> hashes)
    {
        var code = new HashCode();
        foreach (var hash in hashes) code.Add(hash);
        return code.ToHashCode();
    }

    private static int HashExpressionDictionary(Dictionary<string, IrExpressionNode>? dict)
    {
        if (dict == null || dict.Count == 0) return 0;
        return dict.Select(kvp => HashCode.Combine(kvp.Key.GetHashCode(), GetNodeHash(kvp.Value)))
            .Aggregate((sum, i) => unchecked(sum + i));
    }
}
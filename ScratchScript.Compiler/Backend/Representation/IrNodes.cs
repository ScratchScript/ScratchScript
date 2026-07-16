using ScratchScript.Compiler.Frontend.Information;
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Backend.Representation;

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

public abstract record IrNode;

public abstract record IrCommandNode : IrNode;

public abstract record IrExpressionNode : IrNode;

// blocks
public record IrBlockNode(Scope Scope) : IrNode
{
    public IrBlockNode(IEnumerable<IrCommandNode> Commands) : this(new Scope { Body = Commands.ToList() })
    {
    }
}

public record IrProgramNode(IEnumerable<IrBlockNode> Blocks, Dictionary<string, TypedValue> Defines) : IrNode
{
    public Dictionary<string, object> Flags { get; } = [];
}

public record IrFunctionNode(bool Warp, FunctionScope FunctionScope) : IrBlockNode(FunctionScope);

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

public record IrFunctionCallExpressionNode(string Function, Dictionary<string, IrExpressionNode> Arguments)
    : IrExpressionNode;

// mostly for targets which require inserting additional commands before/after the statement (e.g. scratch itself)
public record IrComplexExpressionNode(
    IrExpressionNode Expression,
    IrCommandNode? Dependencies = null,
    IrCommandNode? Cleanup = null)
    : IrExpressionNode;

public record IrObjectLiteralExpressionNode(Dictionary<string, IrExpressionNode> Values) : IrExpressionNode;

// commands
public record IrCommandSequenceNode(IEnumerable<IrCommandNode> Commands) : IrCommandNode;

public record IrSetCommandNode(string Variable, IrExpressionNode Expression) : IrCommandNode;

public record IrCallFunctionCommandNode(string Function, Dictionary<string, IrExpressionNode> Arguments)
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

public record IrFunctionReturnCommandNode(IrExpressionNode? ReturnValue) : IrCommandNode;

public static class IrHasher
{
    public static int GetNodeHash(IrNode? node)
        => node switch
        {
            null => 0,

            // blocks
            IrProgramNode program => HashCode.Combine(1, HashEnumerable(program.Blocks.Select(GetNodeHash))),
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
                HashCode.Combine(13, callExpr.Function.GetHashCode(), HashDictionary(callExpr.Arguments)),
            IrShadowExpressionNode shadow =>
                HashCode.Combine(14, shadow.Opcode.GetHashCode(), shadow.ExpectedType,
                    HashDictionary(shadow.Inputs),
                    HashDictionary(shadow.Fields)),
            IrComplexExpressionNode complex =>
                HashCode.Combine(15, GetNodeHash(complex.Expression), GetNodeHash(complex.Dependencies),
                    GetNodeHash(complex.Cleanup)),
            IrObjectLiteralExpressionNode obj =>
                HashCode.Combine(16, HashDictionary(obj.Values)),

            // commands
            IrCommandSequenceNode seq => HashCode.Combine(17, HashEnumerable(seq.Commands.Select(GetNodeHash))),
            IrSetCommandNode set => HashCode.Combine(18, set.Variable.GetHashCode(),
                GetNodeHash(set.Expression)),
            IrCallFunctionCommandNode callCmd => HashCode.Combine(19, callCmd.Function.GetHashCode(),
                HashDictionary(callCmd.Arguments)),
            IrFunctionReturnCommandNode ret => HashCode.Combine(20, GetNodeHash(ret.ReturnValue)),
            IrRawCommandNode raw =>
                HashCode.Combine(21, raw.Opcode.GetHashCode(), HashDictionary(raw.Inputs),
                    HashDictionary(raw.Fields)),

            // lists
            IrPushCommand push => HashCode.Combine(22, push.List.GetHashCode(), GetNodeHash(push.Expression)),
            IrPushAtCommand pushAt => HashCode.Combine(23, pushAt.List.GetHashCode(), GetNodeHash(pushAt.Where),
                GetNodeHash(pushAt.Expression)),
            IrPopCommand pop => HashCode.Combine(24, pop.List.GetHashCode()),
            IrPopAtCommand popAt => HashCode.Combine(25, popAt.List.GetHashCode(), GetNodeHash(popAt.Where)),
            IrPopAllCommand popAll => HashCode.Combine(26, popAll.List.GetHashCode()),

            // control Flow
            IrIfCommandNode cond => HashCode.Combine(27, GetNodeHash(cond.Condition), GetNodeHash(cond.Body),
                GetNodeHash(cond.Alternate)),
            IrWhileCommandNode loop =>
                HashCode.Combine(28, GetNodeHash(loop.Condition), GetNodeHash(loop.Body)),
            IrRepeatCommandNode repeat => HashCode.Combine(29, GetNodeHash(repeat.Times),
                GetNodeHash(repeat.Body)),

            _ => throw new ArgumentOutOfRangeException(nameof(node), $"Unhandled node type: {node.GetType().Name}")
        };

    private static int HashEnumerable(IEnumerable<int> hashes)
    {
        var code = new HashCode();
        foreach (var hash in hashes) code.Add(hash);
        return code.ToHashCode();
    }

    private static int HashDictionary(Dictionary<string, IrExpressionNode>? dict)
    {
        if (dict == null || dict.Count == 0) return 0;
        return dict.Select(kvp => HashCode.Combine(kvp.Key.GetHashCode(), GetNodeHash(kvp.Value)))
            .Aggregate((sum, i) => unchecked(sum + i));
    }
}
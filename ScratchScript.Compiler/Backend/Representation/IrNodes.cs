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
public record IrBlockNode(Scope Scope) : IrNode;

public record IrProgramNode(IEnumerable<IrBlockNode> Blocks, Dictionary<string, TypedValue> Defines) : IrNode;

public record IrFunctionNode(bool Warp, string Name, FunctionScope FunctionScope) : IrBlockNode(FunctionScope);

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
public record IrComplexExpressionNode(IrExpressionNode Expression, IrCommandNode? Dependencies, IrCommandNode? Cleanup)
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
using ScratchScript.Compiler.Types;

namespace ScratchScript.Compiler.Frontend.Implementation;

public enum AddOperator
{
    Minus,
    Plus
}

public enum BooleanOperator
{
    And,
    Or
}

public enum MultiplyOperators
{
    Multiply,
    Divide,
    Modulus,
    Power
}

public enum CompareOperators
{
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual
}

public enum BitwiseOperators
{
    Or,
    And,
    Xor,
    LeftShift,
    RightShift
}

public enum AssignmentOperator
{
    Assignment,
    AdditionAssignment,
    SubtractionAssignment,
    MultiplicationAssignment,
    DivisionAssignment,
    ModulusAssignment,
    PowerAssignment
}

public static class AssignmentOperatorExtensions
{
    public static MultiplyOperators ToMultiplyOperator(this AssignmentOperator op)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return op switch
        {
            AssignmentOperator.MultiplicationAssignment => MultiplyOperators.Multiply,
            AssignmentOperator.DivisionAssignment => MultiplyOperators.Divide,
            AssignmentOperator.ModulusAssignment => MultiplyOperators.Modulus,
            AssignmentOperator.PowerAssignment => MultiplyOperators.Power,
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }
}

public partial class ScratchScriptVisitor
{
    public override TypedValue? VisitAssignmentOperators(ScratchScriptParser.AssignmentOperatorsContext context)
    {
        if (context.Assignment() != null) return new GenericValue<AssignmentOperator>(AssignmentOperator.Assignment);
        if (context.AdditionAsignment() != null)
            return new GenericValue<AssignmentOperator>(AssignmentOperator.AdditionAssignment);
        if (context.SubtractionAssignment() != null)
            return new GenericValue<AssignmentOperator>(AssignmentOperator.SubtractionAssignment);
        if (context.MultiplicationAssignment() != null)
            return new GenericValue<AssignmentOperator>(AssignmentOperator.MultiplicationAssignment);
        if (context.DivisionAssignment() != null)
            return new GenericValue<AssignmentOperator>(AssignmentOperator.DivisionAssignment);
        if (context.ModulusAssignment() != null)
            return new GenericValue<AssignmentOperator>(AssignmentOperator.ModulusAssignment);
        if (context.PowerAssignment() != null)
            return new GenericValue<AssignmentOperator>(AssignmentOperator.PowerAssignment);
        return null;
    }

    public override TypedValue? VisitAddOperators(ScratchScriptParser.AddOperatorsContext context)
    {
        if (context.Plus() != null) return new GenericValue<AddOperator>(AddOperator.Plus);
        if (context.Minus() != null) return new GenericValue<AddOperator>(AddOperator.Minus);
        return null;
    }

    public override TypedValue? VisitBooleanOperators(ScratchScriptParser.BooleanOperatorsContext context)
    {
        if (context.And() != null) return new GenericValue<BooleanOperator>(BooleanOperator.And);
        if (context.Or() != null) return new GenericValue<BooleanOperator>(BooleanOperator.Or);
        return null;
    }

    public override TypedValue? VisitMultiplyOperators(ScratchScriptParser.MultiplyOperatorsContext context)
    {
        if (context.Multiply() != null) return new GenericValue<MultiplyOperators>(MultiplyOperators.Multiply);
        if (context.Divide() != null) return new GenericValue<MultiplyOperators>(MultiplyOperators.Divide);
        if (context.Modulus() != null) return new GenericValue<MultiplyOperators>(MultiplyOperators.Modulus);
        if (context.Power() != null) return new GenericValue<MultiplyOperators>(MultiplyOperators.Power);
        return null;
    }

    public override TypedValue? VisitCompareOperators(ScratchScriptParser.CompareOperatorsContext context)
    {
        if (context.Lesser() != null) return new GenericValue<CompareOperators>(CompareOperators.LessThan);
        if (context.LesserOrEqual() != null)
            return new GenericValue<CompareOperators>(CompareOperators.LessThanOrEqual);
        if (context.Greater() != null) return new GenericValue<CompareOperators>(CompareOperators.GreaterThan);
        if (context.GreaterOrEqual() != null)
            return new GenericValue<CompareOperators>(CompareOperators.GreaterThanOrEqual);
        if (context.Equal() != null) return new GenericValue<CompareOperators>(CompareOperators.Equal);
        if (context.NotEqual() != null) return new GenericValue<CompareOperators>(CompareOperators.NotEqual);
        return null;
    }

    public override TypedValue? VisitBitwiseOperators(ScratchScriptParser.BitwiseOperatorsContext context)
    {
        if (context.BitwiseAnd() != null) return new GenericValue<BitwiseOperators>(BitwiseOperators.And);
        if (context.BitwiseOr() != null) return new GenericValue<BitwiseOperators>(BitwiseOperators.Or);
        if (context.BitwiseXor() != null) return new GenericValue<BitwiseOperators>(BitwiseOperators.Xor);
        if (context.leftShift() != null) return new GenericValue<BitwiseOperators>(BitwiseOperators.LeftShift);
        if (context.rightShift() != null) return new GenericValue<BitwiseOperators>(BitwiseOperators.RightShift);
        return null;
    }
}
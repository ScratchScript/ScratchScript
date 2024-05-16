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

public partial class ScratchScriptVisitor
{
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
        if (context.LesserOrEqual() != null) return new GenericValue<CompareOperators>(CompareOperators.LessThanOrEqual);
        if (context.Greater() != null) return new GenericValue<CompareOperators>(CompareOperators.GreaterThan);
        if (context.GreaterOrEqual() != null) return new GenericValue<CompareOperators>(CompareOperators.GreaterThanOrEqual);
        if (context.Equal() != null) return new GenericValue<CompareOperators>(CompareOperators.Equal);
        if (context.NotEqual() != null) return new GenericValue<CompareOperators>(CompareOperators.NotEqual);
        return base.VisitCompareOperators(context);
    }
}
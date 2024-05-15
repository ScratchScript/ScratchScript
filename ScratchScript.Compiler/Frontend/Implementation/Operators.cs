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
}
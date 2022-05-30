using ScratchScript.Blocks.Builders;
using ScratchScript.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public class Operators
{
	private static Block Binary(object first, object second, string opcode, string id, string firstParameter = "NUM1", string secondParameter = "NUM2")
	{
		var builder = new BlockBuilder()
			.IsShadow()
			.WithOpcode(opcode)
			.WithId(id);
		switch (first)
		{
			case ScratchVariable firstVariable:
				builder.WithInput(new InputBuilder()
					.WithName(firstParameter)
					.WithVariable(firstVariable));
				break;
			case Block firstShadow:
				builder.WithInput(new InputBuilder()
					.WithName(firstParameter)
					.WithShadow(firstShadow));
				break;
			default:
				builder.WithInput(new InputBuilder()
					.WithName(firstParameter)
					.WithRawObject(first));
				break;
		}

		switch (second)
		{
			case ScratchVariable secondVariable:
				builder.WithInput(new InputBuilder()
					.WithName(secondParameter)
					.WithVariable(secondVariable));
				break;
			case Block secondShadow:
				builder.WithInput(new InputBuilder()
					.WithName(secondParameter)
					.WithShadow(secondShadow));
				break;
			default:
				builder.WithInput(new InputBuilder()
					.WithName(secondParameter)
					.WithRawObject(second));
				break;
		}

		return builder;
	}

	public static Block Add(object first, object second) => Binary(first, second, "operator_add", "OperatorAdd");

	public static Block Subtract(object first, object second) =>
		Binary(first, second, "operator_subtract", "OperatorSubtract");

	public static Block Multiply(object first, object second) =>
		Binary(first, second, "operator_multiply", "OperatorMultiply");

	public static Block Divide(object first, object second) =>
		Binary(first, second, "operator_divide", "OperatorDivide");

	public static Block Modulo(object first, object second) => Binary(first, second, "operator_mod", "OperatorModulo");

	public static Block Join(object first, object second) =>
		Binary(first, second, "operator_join", "OperatorStringJoin", "STRING1", "STRING2");

	public static Block Equals(object first, object second) =>
		Binary(first, second, "operator_equals", "OperatorEquals", "OPERAND1", "OPERAND2");

	public static Block LessThan(object first, object second) =>
		Binary(first, second, "operator_lt", "OperatorLessThan", "OPERAND1", "OPERAND2");

	public static Block GreaterThan(object first, object second) => Binary(first, second, "operator_gt",
		"OperatorGreaterThan", "OPERAND1", "OPERAND2");

	public static Block And(object first, object second) =>
		Binary(first, second, "operator_and", "OperatorAnd", "OPERAND1", "OPERAND2");

	public static Block Or(object first, object second) =>
		Binary(first, second, "operator_or", "OperatorOr", "OPERAND1", "OPERAND2");
}
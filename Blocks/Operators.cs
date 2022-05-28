using ScratchScript.Blocks.Builders;
using ScratchScript.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public class Operators
{
	private static Block Binary(object first, object second, string opcode, string id)
	{
		var builder = new BlockBuilder()
			.IsShadow()
			.WithOpcode(opcode)
			.WithId(id);
		switch (first)
		{
			case ScratchVariable firstVariable:
				builder.WithInput(new InputBuilder()
					.WithName("NUM1")
					.WithVariable(firstVariable));
				break;
			case Block firstShadow:
				builder.WithInput(new InputBuilder()
					.WithName("NUM1")
					.WithShadow(firstShadow));
				break;
			default:
				builder.WithInput(new InputBuilder()
					.WithName("NUM1")
					.WithRawObject(first));
				break;
		}

		switch (second)
		{
			case ScratchVariable secondVariable:
				builder.WithInput(new InputBuilder()
					.WithName("NUM2")
					.WithVariable(secondVariable));
				break;
			case Block secondShadow:
				builder.WithInput(new InputBuilder()
					.WithName("NUM2")
					.WithShadow(secondShadow));
				break;
			default:
				builder.WithInput(new InputBuilder()
					.WithName("NUM2")
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
	
	

}
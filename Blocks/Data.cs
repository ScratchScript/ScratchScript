using ScratchScript.Blocks.Builders;
using ScratchScript.Types;
using ScratchScript.Wrapper;

namespace ScratchScript.Blocks;

public class Data
{
	public static Block SetVariableTo(ScratchVariable target, object? to)
	{
		var builder = new BlockBuilder()
			.WithOpcode("data_setvariableto")
			.IsShadow(false)
			.WithId("SetVariableTo")
			.WithField(new FieldBuilder()
				.WithName("VARIABLE")
				.WithVariable(target));

		switch (to)
		{
			case ScratchVariable variable:
				builder = builder.WithInput(new InputBuilder()
					.WithName("VALUE")
					.WithVariable(variable));
				break;
			case Block shadow:
				builder = builder.WithInput(new InputBuilder()
					.WithName("VALUE")
					.WithShadow(shadow));
				break;
			default:
				builder = builder.WithInput(new InputBuilder()
					.WithName("VALUE")
					.WithRawObject(to));
				break;
		}
		
		return builder;
	}
}
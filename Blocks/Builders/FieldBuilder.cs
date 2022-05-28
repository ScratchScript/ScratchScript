using ScratchScript.Types;

namespace ScratchScript.Blocks.Builders;

public class FieldBuilder
{
	private string _name;
	private List<object> _objects = new();

	public FieldBuilder WithName(string name)
	{
		_name = name;
		return this;
	}

	public FieldBuilder WithVariable(ScratchVariable variable)
	{
		_objects = new List<object>
		{
			variable.Name,
			variable.Id
		};
		return this;
	}

	public KeyValuePair<string, List<object>> Build() => new(_name, _objects);
}
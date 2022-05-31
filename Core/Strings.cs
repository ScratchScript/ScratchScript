namespace ScratchScript.Core;

public class Strings
{
	//could not find suitable type for variable {Variable} and shadow {ShadowId} using _expectedType. Defaulting to string
	public static Dictionary<string, string> Messages = new()
	{
		{"W1", "Division by zero is not recommended."},
		{"E2", "ICE: compiler failed to parse an expression."},
		{"E3", "Variable \"{0}\" was already defined."},
		{"E4", "Attributes must be defined at the beginning of the file."},
		{"W5", "ICW: compiler could not find a suitable type for variable \"{0}\" using _expectedType."},
		{"W6", "ICW: ExpectedType is not recommended to be null."},
		{"E7", "Variable \"{0}\" is not defined."},
		{"E8", "Cannot assign a value of type \"{0}\" to a variable of type \"{1}\"."},
		{"E9", "Unexpected identifier \"{0}\"."},
		{"E10", "Syntax error: {0}"}
	};
}
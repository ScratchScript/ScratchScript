using Antlr4.Runtime.Tree;
using ScratchScript.Compiler;
using Serilog;

namespace ScratchScript.Core;

public class ScratchScriptVisitor: ScratchScriptBaseVisitor<object?>
{
	public override object? VisitAttributeStatement(ScratchScriptParser.AttributeStatementContext context)
	{
		return null;
	}

	public override object? VisitVariableDeclarationStatement(ScratchScriptParser.VariableDeclarationStatementContext context)
	{
		var name = context.Identifier().GetText();
		var expression = context.expression();
		Log.Debug("Found variable declaration ({VariableName}, ID {ExpressionNumber})", name, expression.RuleIndex);
		var target = ProjectCompiler.Current.CurrentTarget;

		if (target.Variables.ContainsKey(name))
			throw new Exception($"Variable {name} was already defined. Did you mean to assign a value?");
		target.CreateVariable(name, Visit(expression));
		return null;
	}

	public override object? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
	{
		Log.Debug("Found paranthesized expression ({Text})", context.GetText());
		return Visit(context.expression());
	}
	
	
	public override object? VisitIdentifierExpression(ScratchScriptParser.IdentifierExpressionContext context)
	{
		Log.Debug("Found identifier ({Text})", context.GetText());
		return context.GetText();
	}

	public override object? VisitConstant(ScratchScriptParser.ConstantContext context)
	{
		Log.Debug("Found constant ({Text})", context.GetText());
		
		if (context.Integer() is { } i)
			return int.Parse(i.GetText());

		if (context.Float() is { } f)
			return float.Parse(f.GetText());

		if (context.String() is { } s)
			return s.GetText()[1..^1];

		if (context.Boolean() is { } b)
			return b.GetText() == "true";
		
		return null;
	}
}
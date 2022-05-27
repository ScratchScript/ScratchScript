using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using ScratchScript.Compiler;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core;

public class ScratchScriptVisitor: ScratchScriptBaseVisitor<object?>
{
    protected override object? DefaultResult => base.DefaultResult;

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

    public override object? VisitConstantExpression([NotNull] ScratchScriptParser.ConstantExpressionContext context)
    {
        return VisitConstant(context.constant());
    }

    public override object? VisitBinaryBooleanExpression([NotNull] ScratchScriptParser.BinaryBooleanExpressionContext context)
    {
        return base.VisitBinaryBooleanExpression(context);
    }

    public override object? VisitFunctionCallExpression([NotNull] ScratchScriptParser.FunctionCallExpressionContext context)
    {
        return base.VisitFunctionCallExpression(context);
    }

    public override object? VisitNotExpression([NotNull] ScratchScriptParser.NotExpressionContext context)
    {
        return base.VisitNotExpression(context);
    }

    public override object? VisitBinaryCompareExpression([NotNull] ScratchScriptParser.BinaryCompareExpressionContext context)
    {
        return base.VisitBinaryCompareExpression(context);
    }

    public override object? VisitBinaryMultiplyExpression([NotNull] ScratchScriptParser.BinaryMultiplyExpressionContext context)
    {
        return base.VisitBinaryMultiplyExpression(context);
    }

    public override object? VisitBinaryAddExpression([NotNull] ScratchScriptParser.BinaryAddExpressionContext context)
    {
        return base.VisitBinaryAddExpression(context);
    }

    public override object? VisitAssignmentStatement([NotNull] ScratchScriptParser.AssignmentStatementContext context)
    {
        return base.VisitAssignmentStatement(context);
    }

    public override object? VisitFunctionCallStatement([NotNull] ScratchScriptParser.FunctionCallStatementContext context)
    {
        return base.VisitFunctionCallStatement(context);
    }

    public override object? VisitIfStatement([NotNull] ScratchScriptParser.IfStatementContext context)
    {
        return base.VisitIfStatement(context);
    }

    public override object? VisitWhileStatement([NotNull] ScratchScriptParser.WhileStatementContext context)
    {
        return base.VisitWhileStatement(context);
    }

    public override object? VisitElseIfStatement([NotNull] ScratchScriptParser.ElseIfStatementContext context)
    {
        return base.VisitElseIfStatement(context);
    }

    public override object? VisitImportStatement([NotNull] ScratchScriptParser.ImportStatementContext context)
    {
        return base.VisitImportStatement(context);
    }

    public override object? VisitExpression([NotNull] ScratchScriptParser.ExpressionContext context)
    {
        return base.VisitExpression(context);
    }

    public override object? VisitMultiplyOperators([NotNull] ScratchScriptParser.MultiplyOperatorsContext context)
    {
        return base.VisitMultiplyOperators(context);
    }

    public override object? VisitAddOperators([NotNull] ScratchScriptParser.AddOperatorsContext context)
    {
        return base.VisitAddOperators(context);
    }

    public override object? VisitCompareOperators([NotNull] ScratchScriptParser.CompareOperatorsContext context)
    {
        return base.VisitCompareOperators(context);
    }

    public override object? VisitBooleanOperators([NotNull] ScratchScriptParser.BooleanOperatorsContext context)
    {
        return base.VisitBooleanOperators(context);
    }

    public override object? VisitBlock([NotNull] ScratchScriptParser.BlockContext context)
    {
        return base.VisitBlock(context);
    }

    public override object? VisitSingleLineComment([NotNull] ScratchScriptParser.SingleLineCommentContext context)
    {
		
        return base.VisitSingleLineComment(context);
    }

    public override object? VisitMultiLineComment([NotNull] ScratchScriptParser.MultiLineCommentContext context)
    {
        return base.VisitMultiLineComment(context);
    }
}
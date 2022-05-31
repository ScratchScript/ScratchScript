using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using ScratchScript.Blocks;
using ScratchScript.Compiler;
using ScratchScript.Extensions;
using ScratchScript.Types;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core;

public class ScratchScriptVisitor : ScratchScriptBaseVisitor<object?>
{
	private readonly Dictionary<string, Type> _expectedType = new();
	private bool _isStage;

	public override object? VisitAttributeStatement(ScratchScriptParser.AttributeStatementContext context)
	{
		Log.Debug("Found an attribute");

		var target = ProjectCompiler.Current.CurrentTarget;
		if (target.WrappedTarget.blocks.Count > 1 || target.Variables.Count != 0)
		{
			DiagnosticReporter.ReportError(context.Start, "E4");
			return null;
		}

		var attribute = context.Identifier().GetText();
		switch (attribute)
		{
			case "stage":
				Log.Debug("Switching to stage sprite");
				_isStage = true;
				ProjectCompiler.Current.SetCurrentTarget("Stage");
				break;
		}

		return null;
	}

	public override object? VisitVariableDeclarationStatement(
		ScratchScriptParser.VariableDeclarationStatementContext context)
	{
		var name = context.Identifier().GetText();
		var expression = context.expression();
		Log.Debug("Found variable declaration ({VariableName}, Expression ID {ExpressionNumber})", name,
			expression.RuleIndex);
		var target = ProjectCompiler.Current.CurrentTarget;

		if (target.Variables.ContainsKey(name))
		{
			DiagnosticReporter.ReportError(context.Identifier().Symbol, "E3", "Did you mean to assign a value?", name);
			return null;
		}

		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Key;
		var expressionResult = Visit(expression);
		if (expressionResult == null)
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E2");
			return null;
		}

		if (expressionResult is not Block expressionBlock)
		{
			target.CreateVariable(name, GetDefaultValue(expressionResult.GetType()));
			var block = target.CreateBlock(Data.SetVariableTo(target.Variables[name], expressionResult));
			block.parent = last;
			target.ReplaceBlock(block);
		}
		else
		{
			Log.Debug("Found shadow in variable declaration ({ShadowId}). Creating SetTo block", expressionBlock.Id);
			if (!_expectedType.ContainsKey(expressionBlock.Id))
			{
				DiagnosticReporter.ReportWarning(expression.Start, "W5", "Defaulting to string.", name);
				target.CreateVariable(name, "");
			}

			var type = _expectedType[expressionBlock.Id];
			target.CreateVariable(name, GetDefaultValue(type));

			var setBlock = target.CreateBlock(Data.SetVariableTo(target.Variables[name], expressionBlock), true, true);
			AttachShadow(setBlock, expressionBlock, last);
		}

		return null;
	}

	private object GetDefaultValue(Type type)
	{
		var defaultValue = Activator.CreateInstance(type);
		if (defaultValue == null)
			throw new Exception($"Cannot get default value for type {type.Name}.");
		Log.Debug("Default value for type {Type} is {Value}", type.Name, defaultValue);
		return defaultValue;
	}

	private Type GetExpectedType(object obj)
	{
		var type = obj switch
		{
			ScratchVariable variable => variable.Type,
			Block block => _expectedType[block.Id],
			_ => null
		};
		if (type != null) return type;
		if (int.TryParse(obj.ToString(), CultureInfo.InvariantCulture, out _))
			type = typeof(int);
		else if (float.TryParse(obj.ToString(), CultureInfo.InvariantCulture, out _))
			type = typeof(float);
		else type = obj.GetType();

		return type;
	}


	public override object? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
	{
		Log.Debug("Found parenthesized expression ({Text})", context.GetText());
		return Visit(context.expression());
	}


	public override object? VisitIdentifierExpression(ScratchScriptParser.IdentifierExpressionContext context)
	{
		var identifier = context.GetText();
		Log.Debug("Found identifier ({Text})", identifier);
		var target = ProjectCompiler.Current.CurrentTarget;
		if (target.Variables.ContainsKey(identifier))
			return target.Variables[identifier];

		DiagnosticReporter.ReportError(context.Start, "E9", "", identifier);
		return null;
	}

	public override object? VisitConstant(ScratchScriptParser.ConstantContext context)
	{
		Log.Debug("Found constant ({Text})", context.GetText());

		if (context.Integer() is { } i)
			return int.Parse(i.GetText());

		if (context.Float() is { } f)
			return float.Parse(f.GetText().Replace("f", ""), CultureInfo.InvariantCulture);

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

	public override object? VisitBinaryBooleanExpression(
		[NotNull] ScratchScriptParser.BinaryBooleanExpressionContext context)
	{
		return base.VisitBinaryBooleanExpression(context);
	}

	public override object? VisitFunctionCallExpression(
		[NotNull] ScratchScriptParser.FunctionCallExpressionContext context)
	{
		return base.VisitFunctionCallExpression(context);
	}

	public override object? VisitNotExpression([NotNull] ScratchScriptParser.NotExpressionContext context)
	{
		return base.VisitNotExpression(context);
	}

	public override object? VisitUnaryAddExpression(ScratchScriptParser.UnaryAddExpressionContext context)
	{
		Log.Debug("Found a unary +/- expression");
		var expressionResult = Visit(context.expression());
		if (expressionResult is null)
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E2");
			return null;
		}

		var op = context.addOperators().GetText();
		var type = GetExpectedType(expressionResult);
		return HandleBinaryOperation(context.addOperators().Start, op, expressionResult,
			Operators.Join(op, expressionResult), type);
	}

	public override object? VisitBinaryCompareExpression(
		[NotNull] ScratchScriptParser.BinaryCompareExpressionContext context)
	{
		//a
		Log.Debug("Found a > / < / == / >= / <= expression");

		var first = Visit(context.expression(0));
		var second = Visit(context.expression(1));
		if (first is null || second is null)
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E2");
			return null;
		}

		var position = context.compareOperators().Start;
		return context.compareOperators().GetText() switch
		{
			"==" => HandleBinaryOperation(position, first, second, Operators.Equals(first, second), typeof(bool)),
			">" => HandleBinaryOperation(position, first, second, Operators.GreaterThan(first, second), typeof(bool)),
			"<" => HandleBinaryOperation(position, first, second, Operators.LessThan(first, second), typeof(bool)),
			">=" => throw new NotImplementedException("Currently not implemented."),
			"<=" => throw new NotImplementedException("Currently not implemented."),
			_ => null
		};
	}

	public override object? VisitFunctionCallStatement(
		[NotNull] ScratchScriptParser.FunctionCallStatementContext context)
	{
		return base.VisitFunctionCallStatement(context);
	}

	public override object? VisitIfStatement([NotNull] ScratchScriptParser.IfStatementContext context)
	{
		//b
		Log.Debug("Found an if statement (Expression ID {Id})", context.expression().RuleIndex);
		var expressionResult = Visit(context.expression());
		if (expressionResult is null)
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E2");
			return null;
		}

		return null;
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

	public override object? VisitBinaryMultiplyExpression(
		[NotNull] ScratchScriptParser.BinaryMultiplyExpressionContext context)
	{
		Log.Debug("Found */(**)% binary expression");
		var first = Visit(context.expression(0));
		var second = Visit(context.expression(1));

		if (first is null || second is null)
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E2");
			return null;
		}

		var firstType = GetExpectedType(first);
		var secondType = GetExpectedType(second);
		Type? expectedType = null;
		if (firstType == secondType)
			expectedType = firstType;
		else if ((firstType == typeof(int) && secondType == typeof(float)) ||
		         (secondType == typeof(int) && firstType == typeof(float)))
			expectedType = typeof(float);

		if (expectedType == null)
		{
			Log.Warning("Could not detect expected result type for a binary expression. Defaulting to int");
			expectedType = typeof(int);
		}

		var position = context.multiplyOperators().Start;
		switch (context.multiplyOperators().GetText())
		{
			case "*":
				return HandleBinaryOperation(position, first, second, Operators.Multiply(first, second), expectedType);
			case "/":
				if (first is 0 || second is 0)
					DiagnosticReporter.ReportWarning(
						first is 0 ? context.expression(0).Start : context.expression(1).Start, "W1");
				return HandleBinaryOperation(position, first, second, Operators.Divide(first, second), expectedType);
			case "**":
				throw new NotImplementedException("Currently not implemented.");
			case "%":
				return HandleBinaryOperation(position, first, second, Operators.Modulo(first, second), expectedType);
		}

		return null;
	}


	public override object? VisitBinaryAddExpression([NotNull] ScratchScriptParser.BinaryAddExpressionContext context)
	{
		Log.Debug("Found +- binary expression");
		var first = Visit(context.expression(0));
		var second = Visit(context.expression(1));

		if (first is null || second is null)
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E2");
			return null;
		}

		var position = context.addOperators().Start;
		switch (context.addOperators().GetText())
		{
			case "+":
				//TODO: join strings (which is a separate block)
				return HandleBinaryOperation(position, first, second, Operators.Add(first, second), typeof(int));
			case "-":
				return HandleBinaryOperation(position, first, second, Operators.Subtract(first, second), typeof(int));
		}

		return null;
	}

	private object HandleBinaryOperation(IToken position, object? first, object? second, Block operatorBlock,
		Type? expectedType = null)
	{
		var target = ProjectCompiler.Current.CurrentTarget;
		var block = target.CreateBlock(operatorBlock, true, true);
		block.next = null;
		if (expectedType == null)
			DiagnosticReporter.ReportWarning(position, "W6");
		else _expectedType[block.Id] = expectedType;
		if (first is Block firstBlock)
		{
			firstBlock.parent = block.Id;
			target.ReplaceBlock(block);
			target.ReplaceBlock(firstBlock);
		}
		else if (second is Block secondBlock)
		{
			secondBlock.parent = block.Id;
			target.ReplaceBlock(block);
			target.ReplaceBlock(secondBlock);
		}

		return block;
	}

	public override object? VisitAssignmentStatement([NotNull] ScratchScriptParser.AssignmentStatementContext context)
	{
		var name = context.Identifier().GetText();
		Log.Debug("Found variable assignment ({Name}, Expression ID {Value})", name, context.expression().RuleIndex);
		var target = ProjectCompiler.Current.CurrentTarget;

		if (!target.Variables.ContainsKey(name))
		{
			DiagnosticReporter.ReportError(context.Identifier().Symbol, "E7");
			return null;
		}

		var variable = target.Variables[name];
		var value = Visit(context.expression());
		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Key;
		if (value == null)
		{
			DiagnosticReporter.ReportError(context.FirstParentOfType<ScratchScriptParser.LineContext>().Start, "E2");
			return null;
		}

		if (GetExpectedType(variable) != GetExpectedType(value))
		{
			DiagnosticReporter.ReportError(context.assignmentOperators().Start, "E8", "", GetExpectedType(value).Name,
				GetExpectedType(variable).Name);
			return null;
		}

		var op = context.assignmentOperators().GetText();

		var shadow = op switch
		{
			"=" => null,
			"+=" => Operators.Add(variable, value),
			"-=" => Operators.Subtract(variable, value),
			"*=" => Operators.Multiply(variable, value),
			"/=" => Operators.Divide(variable, value),
			"%=" => Operators.Modulo(variable, value),
			_ => null!
		};

		if (shadow is null && value is Block block)
			shadow = block;
		if (shadow is not null)
		{
			var setBlock = target.CreateBlock(Data.SetVariableTo(variable, shadow));
			AttachShadow(setBlock, shadow, last);
		}
		else
		{
			target.CreateBlock(Data.SetVariableTo(variable, value));
		}


		return null;
	}

	private void AttachShadow(Block main, Block shadow, string blockBeforeMain)
	{
		var target = ProjectCompiler.Current.CurrentTarget;
		shadow.parent = main.Id;
		main.parent = blockBeforeMain;
		target.WrappedTarget.blocks[blockBeforeMain].next = main.Id;
		target.ReplaceBlock(main);
		target.ReplaceBlock(shadow);
	}

	public override object? VisitMultiplyOperators([NotNull] ScratchScriptParser.MultiplyOperatorsContext context)
	{
		Log.Debug("Found */(**)% arithmetic operator");
		return context.GetText();
	}

	public override object? VisitAddOperators([NotNull] ScratchScriptParser.AddOperatorsContext context)
	{
		Log.Debug("Found +- arithmetic operator");
		return context.GetText();
	}

	public override object? VisitCompareOperators([NotNull] ScratchScriptParser.CompareOperatorsContext context)
	{
		Log.Debug("Found == != > >= < <= boolean operator");
		return context.GetText();
	}

	public override object? VisitBooleanOperators([NotNull] ScratchScriptParser.BooleanOperatorsContext context)
	{
		Log.Debug("Found && || ^ boolean operator");
		return context.GetText();
	}

	public override object? VisitComment(ScratchScriptParser.CommentContext context)
	{
		var target = ProjectCompiler.Current.CurrentTarget;
		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Value;
		Log.Debug("Found a comment. Attaching to next block after {LastBlockId}", last.Id);
		var text = context.GetText().EndsWith("*/") ? context.GetText()[2..^2] : context.GetText()[2..];
		var comment = new Comment
		{
			minimized = false,
			text = text,
			width = text.Length * 15,
			height = 75,
			x = -100,
			y = -100
		};
		var id = BlockExtensions.RandomId("Comment");
		target.PendingComment = id;
		target.WrappedTarget.comments[id] = comment;
		return null;
	}
}
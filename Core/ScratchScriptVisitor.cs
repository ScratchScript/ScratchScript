using System.Globalization;
using Antlr4.Runtime.Misc;
using ScratchScript.Blocks;
using ScratchScript.Compiler;
using ScratchScript.Extensions;
using ScratchScript.Helpers;
using ScratchScript.Types;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core;

public class ScratchScriptVisitor: ScratchScriptBaseVisitor<object?>
{
	private Dictionary<string, Type> _expectedType = new();
	private bool _isStage;

	public override object? VisitAttributeStatement(ScratchScriptParser.AttributeStatementContext context)
	{
		Log.Debug("Found an attribute");

		var target = ProjectCompiler.Current.CurrentTarget;
		if (target.WrappedTarget.blocks.Count > 1 || target.Variables.Count != 0)
			throw new Exception("Attributes must be defined at the beginning of the file.");

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

	public override object? VisitVariableDeclarationStatement(ScratchScriptParser.VariableDeclarationStatementContext context)
	{
		var name = context.Identifier().GetText();
		var expression = context.expression();
		Log.Debug("Found variable declaration ({VariableName}, ID {ExpressionNumber})", name, expression.RuleIndex);
		var target = ProjectCompiler.Current.CurrentTarget;

		if (target.Variables.ContainsKey(name))
			throw new Exception($"Variable {name} was already defined. Did you mean to assign a value?");

		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Key;
		var expressionResult = Visit(expression);
		if (expressionResult == null) throw new Exception("That was not supposed to happen.");
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
				Log.Warning("Internal compiler warning: could not find suitable type for variable {Variable} and shadow {ShadowId} using _expectedType. Defaulting to string", name, expressionBlock.Id);
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
		return obj switch
		{
			ScratchVariable variable => variable.Type,
			Block block => _expectedType[block.Id],
			_ => obj.GetType()
		};
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

		throw new Exception($"Unexpected identifier \"{identifier}\".");
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

    public override object? VisitUnaryAddExpression(ScratchScriptParser.UnaryAddExpressionContext context)
    {
	    Log.Debug("Found a unary +/- expression");
	    var expressionResult = Visit(context.expression());
	    if (expressionResult is null) throw new Exception("oh no");
	    var op = context.addOperators().GetText();
	    var type = GetExpectedType(expressionResult);
	    return HandleBinaryOperation(op, expressionResult, Operators.Join(op, expressionResult), type);
    }

    public override object? VisitBinaryCompareExpression([NotNull] ScratchScriptParser.BinaryCompareExpressionContext context)
    {
        return base.VisitBinaryCompareExpression(context);
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

    public override object? VisitBinaryMultiplyExpression([NotNull] ScratchScriptParser.BinaryMultiplyExpressionContext context)
    {
	    Log.Debug("Found */(**)% binary expression");
	    var first = Visit(context.expression(0));
	    var second = Visit(context.expression(1));

	    if (first is null || second is null)
		    throw new Exception("Internal compiler error: VisitBinaryMultiplyExpression() failed to parse expressions.");
	    
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
		    
	    switch (context.multiplyOperators().GetText())
	    {
		    case "*":
			    return HandleBinaryOperation(first, second, Operators.Multiply(first, second), expectedType);
		    case "/":
			    if(first is 0 || second is 0)
					Log.Warning("Division by zero is not recommended. It is not fatal, but Scratch will always return infinity");
			    return HandleBinaryOperation(first, second, Operators.Divide(first, second), expectedType);
		    case "**":
			    throw new NotImplementedException("Currently not implemented.");
		    case "%":
			    return HandleBinaryOperation(first, second, Operators.Modulo(first, second), expectedType);
	    }

	    throw new Exception("What?");
    }
    

    public override object? VisitBinaryAddExpression([NotNull] ScratchScriptParser.BinaryAddExpressionContext context)
    {
	    Log.Debug("Found +- binary expression");
        var first = Visit(context.expression(0));
        var second = Visit(context.expression(1));

        if (first is null || second is null)
	        throw new Exception("Internal compiler error: VisitBinaryAddExpression() failed to parse expressions.");
        
        switch (context.addOperators().GetText())
        {
	        case "+":
		        //TODO: join strings (which is a separate block)
		        return HandleBinaryOperation(first, second, Operators.Add(first, second), typeof(int));
	        case "-":
		        return HandleBinaryOperation(first, second, Operators.Subtract(first, second), typeof(int));
        }

        throw new Exception("What?");
    }

    private object HandleBinaryOperation(object? first, object? second, Block operatorBlock, Type? expectedType = null)
    {
	    var target = ProjectCompiler.Current.CurrentTarget;
	    var block = target.CreateBlock(operatorBlock, true, true);
	    block.next = null;
	    if(expectedType == null)
		    Log.Warning("Internal compiler warning: ExpectedType is not recommended to be null, since many type checks might fail");
	    else _expectedType[block.Id] = expectedType;
	    if (first is Block firstBlock)
	    {
		    firstBlock.parent = block.Id;
		    target.ReplaceBlock(block);
		    target.ReplaceBlock(firstBlock);
	    }
	    else if(second is Block secondBlock)
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
		Log.Debug("Found variable assignment ({Name}, ID {Value})", name, context.expression().RuleIndex);
		var target = ProjectCompiler.Current.CurrentTarget;

		if (!target.Variables.ContainsKey(name))
			throw new Exception($"Variable {name} is not defined.");
		
		var variable = target.Variables[name];
		var value = Visit(context.expression());
		var last = target.WrappedTarget.blocks.Last(x => !x.Value.shadow).Key;
		if (value == null) throw new Exception("oops");
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

		/*switch (value)
		{
			case null:
				throw new Exception("Expression result is not assignable to a variable.");
			case ScratchVariable toVariable:
				target.CreateBlock(Data.SetVariableTo(variable, toVariable));
				break;
			case Block shadow:
				var setBlock = target.CreateBlock(Data.SetVariableTo(variable, shadow), true, true);
				AttachShadow(setBlock, shadow, last);
				break;
			default:
				target.CreateBlock(Data.SetVariableTo(variable, value));
				break;
		}*/

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
	    var text = context.GetText().EndsWith("*/") ? context.GetText()[2..^2]: context.GetText()[2..];
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
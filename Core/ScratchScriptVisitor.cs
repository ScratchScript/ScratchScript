using Antlr4.Runtime.Misc;
using ScratchScript.Blocks;
using ScratchScript.Compiler;
using ScratchScript.Extensions;
using ScratchScript.Types;
using ScratchScript.Wrapper;
using Serilog;

namespace ScratchScript.Core;

public class ScratchScriptVisitor: ScratchScriptBaseVisitor<object?>
{
	private Dictionary<string, Type> _expectedType = new();

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

		var last = target.WrappedTarget.blocks.Last().Key;
		var expressionResult = Visit(expression);
		if (expressionResult == null) throw new Exception("That was not supposed to happen.");
		if(expressionResult is not Block expressionBlock)
			target.CreateVariable(name, expressionResult);
		else
		{
			Log.Debug("Found shadow in variable declaration ({ShadowId}). Creating SetTo block", expressionBlock.Id);
			if (!_expectedType.ContainsKey(expressionBlock.Id))
			{
				Log.Warning("Internal compiler warning: could not find suitable type for variable {Variable} and shadow {ShadowId} using _expectedType. Defaulting to string", name, expressionBlock.Id);
				target.CreateVariable(name, "");
			}

			var type = _expectedType[expressionBlock.Id];
			var defaultValue = Activator.CreateInstance(type);
			if (defaultValue == null)
				throw new Exception($"Cannot get default value for type {type.Name}.");
			Log.Debug("Default value for type {Type} is {Value}", type.Name, defaultValue);
			target.CreateVariable(name, defaultValue);
			
			var setBlock = target.CreateBlock(Data.SetVariableTo(target.Variables[name], expressionBlock), true, true);
			AttachShadow(setBlock, expressionBlock, last);
		}
		return null;
	}

	public override object? VisitParenthesizedExpression(ScratchScriptParser.ParenthesizedExpressionContext context)
	{
		Log.Debug("Found paranthesized expression ({Text})", context.GetText());
		return Visit(context.expression());
	}
	
	
	public override object? VisitIdentifierExpression(ScratchScriptParser.IdentifierExpressionContext context)
	{
		var identifier = context.GetText();
		Log.Debug("Found identifier ({Text})", identifier);
		var target = ProjectCompiler.Current.CurrentTarget;
		if (target.Variables.ContainsKey(identifier))
			return target.Variables[identifier];

		return null;
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
	    Log.Debug("Found */÷/^/% binary expression");
	    var first = Visit(context.expression(0));
	    var second = Visit(context.expression(1));
        
	    switch (context.multiplyOperators().GetText())
	    {
		    case "*":
			    return HandleBinaryOperation(first, second, Operators.Multiply(first, second), typeof(int));
		    case "/":
			    return HandleBinaryOperation(first, second, Operators.Divide(first, second), typeof(int));
		    case "**":
			    throw new NotImplementedException("Currently not implemented.");
		    case "%":
			    return HandleBinaryOperation(first, second, Operators.Modulo(first, second), typeof(int));
	    }

	    throw new Exception("What?");
    }
    

    public override object? VisitBinaryAddExpression([NotNull] ScratchScriptParser.BinaryAddExpressionContext context)
    {
	    Log.Debug("Found +/- binary expression");
        var first = Visit(context.expression(0));
        var second = Visit(context.expression(1));
        
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
		var last = target.WrappedTarget.blocks.Last().Key;
		var value = Visit(context.expression());

		switch (value)
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
	    Log.Debug("Found +/- arithmetic operator");
	    return context.GetText();
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


    public override object? VisitComment(ScratchScriptParser.CommentContext context)
    {
	    var target = ProjectCompiler.Current.CurrentTarget;
	    var last = target.WrappedTarget.blocks.Last().Value;
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
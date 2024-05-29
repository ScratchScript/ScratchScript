//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from ../Resources/Grammar/ScratchIR.g4 by ANTLR 4.13.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="ScratchIRParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.CLSCompliant(false)]
public interface IScratchIRListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterProgram([NotNull] ScratchIRParser.ProgramContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitProgram([NotNull] ScratchIRParser.ProgramContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>setCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSetCommand([NotNull] ScratchIRParser.SetCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>setCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSetCommand([NotNull] ScratchIRParser.SetCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>loadCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterLoadCommand([NotNull] ScratchIRParser.LoadCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>loadCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitLoadCommand([NotNull] ScratchIRParser.LoadCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>whileCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterWhileCommand([NotNull] ScratchIRParser.WhileCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>whileCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitWhileCommand([NotNull] ScratchIRParser.WhileCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>repeatCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRepeatCommand([NotNull] ScratchIRParser.RepeatCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>repeatCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRepeatCommand([NotNull] ScratchIRParser.RepeatCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>ifCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIfCommand([NotNull] ScratchIRParser.IfCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>ifCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIfCommand([NotNull] ScratchIRParser.IfCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>callCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCallCommand([NotNull] ScratchIRParser.CallCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>callCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCallCommand([NotNull] ScratchIRParser.CallCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>rawCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRawCommand([NotNull] ScratchIRParser.RawCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>rawCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRawCommand([NotNull] ScratchIRParser.RawCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>pushCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPushCommand([NotNull] ScratchIRParser.PushCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>pushCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPushCommand([NotNull] ScratchIRParser.PushCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>pushAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPushAtCommand([NotNull] ScratchIRParser.PushAtCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>pushAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPushAtCommand([NotNull] ScratchIRParser.PushAtCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>popCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPopCommand([NotNull] ScratchIRParser.PopCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>popCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPopCommand([NotNull] ScratchIRParser.PopCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>popAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPopAtCommand([NotNull] ScratchIRParser.PopAtCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>popAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPopAtCommand([NotNull] ScratchIRParser.PopAtCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>popAllCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPopAllCommand([NotNull] ScratchIRParser.PopAllCommandContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>popAllCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPopAllCommand([NotNull] ScratchIRParser.PopAllCommandContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>functionBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionBlock([NotNull] ScratchIRParser.FunctionBlockContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>functionBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionBlock([NotNull] ScratchIRParser.FunctionBlockContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>eventBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEventBlock([NotNull] ScratchIRParser.EventBlockContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>eventBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEventBlock([NotNull] ScratchIRParser.EventBlockContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>flagTopLevelStatement</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFlagTopLevelStatement([NotNull] ScratchIRParser.FlagTopLevelStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>flagTopLevelStatement</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFlagTopLevelStatement([NotNull] ScratchIRParser.FlagTopLevelStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>constantExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterConstantExpression([NotNull] ScratchIRParser.ConstantExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>constantExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitConstantExpression([NotNull] ScratchIRParser.ConstantExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>variableExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableExpression([NotNull] ScratchIRParser.VariableExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>variableExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableExpression([NotNull] ScratchIRParser.VariableExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>arrayExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterArrayExpression([NotNull] ScratchIRParser.ArrayExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>arrayExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitArrayExpression([NotNull] ScratchIRParser.ArrayExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>argumentExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterArgumentExpression([NotNull] ScratchIRParser.ArgumentExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>argumentExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitArgumentExpression([NotNull] ScratchIRParser.ArgumentExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>parenthesizedExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterParenthesizedExpression([NotNull] ScratchIRParser.ParenthesizedExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>parenthesizedExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitParenthesizedExpression([NotNull] ScratchIRParser.ParenthesizedExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryAddExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBinaryAddExpression([NotNull] ScratchIRParser.BinaryAddExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryAddExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBinaryAddExpression([NotNull] ScratchIRParser.BinaryAddExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryMultiplyExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBinaryMultiplyExpression([NotNull] ScratchIRParser.BinaryMultiplyExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryMultiplyExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBinaryMultiplyExpression([NotNull] ScratchIRParser.BinaryMultiplyExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryBooleanExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBinaryBooleanExpression([NotNull] ScratchIRParser.BinaryBooleanExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryBooleanExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBinaryBooleanExpression([NotNull] ScratchIRParser.BinaryBooleanExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryCompareExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBinaryCompareExpression([NotNull] ScratchIRParser.BinaryCompareExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryCompareExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBinaryCompareExpression([NotNull] ScratchIRParser.BinaryCompareExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>rawShadowExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRawShadowExpression([NotNull] ScratchIRParser.RawShadowExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>rawShadowExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRawShadowExpression([NotNull] ScratchIRParser.RawShadowExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>notExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNotExpression([NotNull] ScratchIRParser.NotExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>notExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNotExpression([NotNull] ScratchIRParser.NotExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by the <c>listAccessExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterListAccessExpression([NotNull] ScratchIRParser.ListAccessExpressionContext context);
	/// <summary>
	/// Exit a parse tree produced by the <c>listAccessExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitListAccessExpression([NotNull] ScratchIRParser.ListAccessExpressionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.elseIfStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElseIfStatement([NotNull] ScratchIRParser.ElseIfStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.elseIfStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElseIfStatement([NotNull] ScratchIRParser.ElseIfStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.ifStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterIfStatement([NotNull] ScratchIRParser.IfStatementContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.ifStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitIfStatement([NotNull] ScratchIRParser.IfStatementContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.callFunctionArgument"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCallFunctionArgument([NotNull] ScratchIRParser.CallFunctionArgumentContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.callFunctionArgument"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCallFunctionArgument([NotNull] ScratchIRParser.CallFunctionArgumentContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.functionArgumentType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterFunctionArgumentType([NotNull] ScratchIRParser.FunctionArgumentTypeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.functionArgumentType"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitFunctionArgumentType([NotNull] ScratchIRParser.FunctionArgumentTypeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.variableIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterVariableIdentifier([NotNull] ScratchIRParser.VariableIdentifierContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.variableIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitVariableIdentifier([NotNull] ScratchIRParser.VariableIdentifierContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.arrayIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterArrayIdentifier([NotNull] ScratchIRParser.ArrayIdentifierContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.arrayIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitArrayIdentifier([NotNull] ScratchIRParser.ArrayIdentifierContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.argumentIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterArgumentIdentifier([NotNull] ScratchIRParser.ArgumentIdentifierContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.argumentIdentifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitArgumentIdentifier([NotNull] ScratchIRParser.ArgumentIdentifierContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.constant"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterConstant([NotNull] ScratchIRParser.ConstantContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.constant"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitConstant([NotNull] ScratchIRParser.ConstantContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.addOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAddOperators([NotNull] ScratchIRParser.AddOperatorsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.addOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAddOperators([NotNull] ScratchIRParser.AddOperatorsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.multiplyOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMultiplyOperators([NotNull] ScratchIRParser.MultiplyOperatorsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.multiplyOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMultiplyOperators([NotNull] ScratchIRParser.MultiplyOperatorsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.booleanOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBooleanOperators([NotNull] ScratchIRParser.BooleanOperatorsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.booleanOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBooleanOperators([NotNull] ScratchIRParser.BooleanOperatorsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.compareOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompareOperators([NotNull] ScratchIRParser.CompareOperatorsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.compareOperators"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompareOperators([NotNull] ScratchIRParser.CompareOperatorsContext context);
}

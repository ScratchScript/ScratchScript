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
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="IScratchIRListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.1")]
[System.Diagnostics.DebuggerNonUserCode]
[System.CLSCompliant(false)]
public partial class ScratchIRBaseListener : IScratchIRListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.program"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterProgram([NotNull] ScratchIRParser.ProgramContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.program"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitProgram([NotNull] ScratchIRParser.ProgramContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>setCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSetCommand([NotNull] ScratchIRParser.SetCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>setCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSetCommand([NotNull] ScratchIRParser.SetCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>loadCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterLoadCommand([NotNull] ScratchIRParser.LoadCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>loadCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitLoadCommand([NotNull] ScratchIRParser.LoadCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>whileCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterWhileCommand([NotNull] ScratchIRParser.WhileCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>whileCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitWhileCommand([NotNull] ScratchIRParser.WhileCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>repeatCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRepeatCommand([NotNull] ScratchIRParser.RepeatCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>repeatCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRepeatCommand([NotNull] ScratchIRParser.RepeatCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>ifCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIfCommand([NotNull] ScratchIRParser.IfCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>ifCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIfCommand([NotNull] ScratchIRParser.IfCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>callCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCallCommand([NotNull] ScratchIRParser.CallCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>callCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCallCommand([NotNull] ScratchIRParser.CallCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>rawCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRawCommand([NotNull] ScratchIRParser.RawCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>rawCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRawCommand([NotNull] ScratchIRParser.RawCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>pushCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPushCommand([NotNull] ScratchIRParser.PushCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>pushCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPushCommand([NotNull] ScratchIRParser.PushCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>pushAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPushAtCommand([NotNull] ScratchIRParser.PushAtCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>pushAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPushAtCommand([NotNull] ScratchIRParser.PushAtCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>popCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPopCommand([NotNull] ScratchIRParser.PopCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>popCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPopCommand([NotNull] ScratchIRParser.PopCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>popAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPopAtCommand([NotNull] ScratchIRParser.PopAtCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>popAtCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPopAtCommand([NotNull] ScratchIRParser.PopAtCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>popAllCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPopAllCommand([NotNull] ScratchIRParser.PopAllCommandContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>popAllCommand</c>
	/// labeled alternative in <see cref="ScratchIRParser.command"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPopAllCommand([NotNull] ScratchIRParser.PopAllCommandContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>functionBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFunctionBlock([NotNull] ScratchIRParser.FunctionBlockContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>functionBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFunctionBlock([NotNull] ScratchIRParser.FunctionBlockContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>eventBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterEventBlock([NotNull] ScratchIRParser.EventBlockContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>eventBlock</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitEventBlock([NotNull] ScratchIRParser.EventBlockContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>flagTopLevelStatement</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFlagTopLevelStatement([NotNull] ScratchIRParser.FlagTopLevelStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>flagTopLevelStatement</c>
	/// labeled alternative in <see cref="ScratchIRParser.block"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFlagTopLevelStatement([NotNull] ScratchIRParser.FlagTopLevelStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>constantExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterConstantExpression([NotNull] ScratchIRParser.ConstantExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>constantExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitConstantExpression([NotNull] ScratchIRParser.ConstantExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>variableExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariableExpression([NotNull] ScratchIRParser.VariableExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>variableExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariableExpression([NotNull] ScratchIRParser.VariableExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>arrayExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArrayExpression([NotNull] ScratchIRParser.ArrayExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>arrayExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArrayExpression([NotNull] ScratchIRParser.ArrayExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>parenthesizedExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterParenthesizedExpression([NotNull] ScratchIRParser.ParenthesizedExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>parenthesizedExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitParenthesizedExpression([NotNull] ScratchIRParser.ParenthesizedExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryAddExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBinaryAddExpression([NotNull] ScratchIRParser.BinaryAddExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryAddExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBinaryAddExpression([NotNull] ScratchIRParser.BinaryAddExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryMultiplyExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBinaryMultiplyExpression([NotNull] ScratchIRParser.BinaryMultiplyExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryMultiplyExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBinaryMultiplyExpression([NotNull] ScratchIRParser.BinaryMultiplyExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryBooleanExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBinaryBooleanExpression([NotNull] ScratchIRParser.BinaryBooleanExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryBooleanExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBinaryBooleanExpression([NotNull] ScratchIRParser.BinaryBooleanExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>binaryCompareExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBinaryCompareExpression([NotNull] ScratchIRParser.BinaryCompareExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>binaryCompareExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBinaryCompareExpression([NotNull] ScratchIRParser.BinaryCompareExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>rawShadowExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRawShadowExpression([NotNull] ScratchIRParser.RawShadowExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>rawShadowExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRawShadowExpression([NotNull] ScratchIRParser.RawShadowExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>notExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNotExpression([NotNull] ScratchIRParser.NotExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>notExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNotExpression([NotNull] ScratchIRParser.NotExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by the <c>listAccessExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterListAccessExpression([NotNull] ScratchIRParser.ListAccessExpressionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by the <c>listAccessExpression</c>
	/// labeled alternative in <see cref="ScratchIRParser.expression"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitListAccessExpression([NotNull] ScratchIRParser.ListAccessExpressionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.elseIfStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterElseIfStatement([NotNull] ScratchIRParser.ElseIfStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.elseIfStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitElseIfStatement([NotNull] ScratchIRParser.ElseIfStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.ifStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterIfStatement([NotNull] ScratchIRParser.IfStatementContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.ifStatement"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitIfStatement([NotNull] ScratchIRParser.IfStatementContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.callFunctionArgument"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCallFunctionArgument([NotNull] ScratchIRParser.CallFunctionArgumentContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.callFunctionArgument"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCallFunctionArgument([NotNull] ScratchIRParser.CallFunctionArgumentContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.functionArgumentType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterFunctionArgumentType([NotNull] ScratchIRParser.FunctionArgumentTypeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.functionArgumentType"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitFunctionArgumentType([NotNull] ScratchIRParser.FunctionArgumentTypeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.variableIdentifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterVariableIdentifier([NotNull] ScratchIRParser.VariableIdentifierContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.variableIdentifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitVariableIdentifier([NotNull] ScratchIRParser.VariableIdentifierContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.arrayIdentifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterArrayIdentifier([NotNull] ScratchIRParser.ArrayIdentifierContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.arrayIdentifier"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitArrayIdentifier([NotNull] ScratchIRParser.ArrayIdentifierContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.constant"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterConstant([NotNull] ScratchIRParser.ConstantContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.constant"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitConstant([NotNull] ScratchIRParser.ConstantContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.addOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAddOperators([NotNull] ScratchIRParser.AddOperatorsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.addOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAddOperators([NotNull] ScratchIRParser.AddOperatorsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.multiplyOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMultiplyOperators([NotNull] ScratchIRParser.MultiplyOperatorsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.multiplyOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMultiplyOperators([NotNull] ScratchIRParser.MultiplyOperatorsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.booleanOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBooleanOperators([NotNull] ScratchIRParser.BooleanOperatorsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.booleanOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBooleanOperators([NotNull] ScratchIRParser.BooleanOperatorsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="ScratchIRParser.compareOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCompareOperators([NotNull] ScratchIRParser.CompareOperatorsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="ScratchIRParser.compareOperators"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCompareOperators([NotNull] ScratchIRParser.CompareOperatorsContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}

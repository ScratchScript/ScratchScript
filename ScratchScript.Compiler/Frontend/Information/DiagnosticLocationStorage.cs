﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace ScratchScript.Compiler.Frontend.Information;

public record DiagnosticLocationStorage
{
    public Dictionary<string, EnumLocationInformation> Enums { get; } = new();
    public Dictionary<string, EventLocationInformation> Events { get; } = new();
    public Dictionary<int, Dictionary<string, VariableLocationInformation>> Variables { get; } = new();
}

public record struct VariableLocationInformation
{
    public ParserRuleContext Context;
    public ITerminalNode Identifier;
    public ParserRuleContext TypeSetterExpression;
}

public record struct EventLocationInformation
{
    public ParserRuleContext Context;
    public ITerminalNode Identifier;
}

public record struct EnumLocationInformation
{
    public ParserRuleContext Context;
    public Dictionary<string, (ParserRuleContext Statement, ParserRuleContext? Assignment)> EntryDeclarations;
    public ITerminalNode Identifier;
    public ParserRuleContext TypeSetterAssignment;
    public ParserRuleContext TypeSetterStatement;
}
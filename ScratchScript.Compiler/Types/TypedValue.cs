﻿using ScratchScript.Compiler.Frontend.Information;

namespace ScratchScript.Compiler.Types;

public record TypedValue(object? Value, ScratchType Type);

public record TypeDeclarationValue(ScratchType Type) : TypedValue(null, Type);

public record EnumEntryValue(string Name, object? Value, ScratchType Type) : TypedValue(Value, Type);

public record ExpressionValue(object? Value, ScratchType Type, string Dependencies = "", string Cleanup = ""): TypedValue(Value, Type);

public record ScopeValue(Scope Scope) : TypedValue(null, ScratchType.Unknown);

// identifier value, list expression value, function argument value, ...
//public record IdentifierValue(): TypedValue();
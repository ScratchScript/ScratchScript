﻿namespace ScratchScript.Compiler.Diagnostics;

public enum ScratchScriptError
{
    ExpectedNonNull = 1,
    TypeMismatch,
    EnumEntryAlreadyDeclared,
    NonNumericEntryMustSpecifyAllValues,
    EventAlreadyDeclared,
    IdentifierAlreadyClaimed
}

public enum ScratchScriptWarning
{
}

public enum ScratchScriptNote
{
    EnumTypeSetAt = 1,
    EnumEntryDeclaredAt,
    EventDeclaredAt,
    IdentifierClaimedAt
}
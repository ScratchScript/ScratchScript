namespace ScratchScript.Compiler.Diagnostics;

public enum ScratchScriptError
{
    ExpectedNonNull = 1,
    TypeMismatch,
    EnumEntryAlreadyDeclared,
    NonNumericEntryMustSpecifyAllValues,
    EventAlreadyDeclared,
    IdentifierAlreadyClaimed,
    UnknownIdentifier,
    VariableNotDefined,
    ArgumentTypeMustBeSpecifiedManually,
    ReturnUsedInNonFunctionContext,
    ReturnWithExpressionOfUnknownType,
    NoFunctionsWithNameAreDefined,
    NoFunctionWithMatchingSignatureDefined,
    FunctionDoesNotReturnAValue,
    ExpectedStatement,
    ExpectedExpression,
    NoAttributeWithMatchingSignatureFound,
    CannotAssignFunctionArgumentInInlinedFunction,
    EnumEntryNotFound,
    NoPropertyDefined
}

public enum ScratchScriptWarning
{
    DivisionByZero = 1,
    UnusedFunctionReturnValue
}

public enum ScratchScriptNote
{
    EnumTypeSetAt = 1,
    EnumEntryDeclaredAt,
    EventDeclaredAt,
    IdentifierClaimedAt,
    VariableTypeSetAt,
    ReturnTypeSetAt,
    FunctionArgumentTypeSetAt,
    ListEnumEntries,
    ListEnumProperties
}
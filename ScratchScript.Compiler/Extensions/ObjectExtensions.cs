namespace ScratchScript.Compiler.Extensions;

public static class ObjectExtensions
{
    public static T CastOrThrow<T>(this object? obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj is not T castedObject ? throw new ArgumentException(null, nameof(obj)) : castedObject;
    }
}
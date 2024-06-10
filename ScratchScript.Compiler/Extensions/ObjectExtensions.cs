namespace ScratchScript.Compiler.Extensions;

public static class ObjectExtensions
{
    public static T CastOrThrow<T>(this object? obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        if (obj is not T castedObject) throw new ArgumentException(nameof(obj));
        return castedObject;
    }
}
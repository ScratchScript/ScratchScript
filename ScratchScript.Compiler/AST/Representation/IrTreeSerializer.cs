using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using ScratchScript.Compiler.TypeChecker;

namespace ScratchScript.Compiler.AST.Representation;

public class PolymorphicNodeFormatter<T> : IMessagePackFormatter<T?> where T : class
{
    private readonly Dictionary<Type, ushort> _typeToId = [];
    private readonly Dictionary<ushort, Type> _idToType = [];

    public PolymorphicNodeFormatter()
    {
        var subtypes = typeof(IrNode).Assembly.GetTypes()
            .Where(t => typeof(IrNode).IsAssignableFrom(t) && !t.IsAbstract)
            .OrderBy(t => t.FullName)
            .ToList();

        for (ushort i = 0; i < subtypes.Count; i++)
        {
            _typeToId[subtypes[i]] = i;
            _idToType[i] = subtypes[i];
        }
    }

    public void Serialize(ref MessagePackWriter writer, T? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        if (!_typeToId.TryGetValue(value.GetType(), out var id))
            throw new InvalidOperationException();

        writer.WriteArrayHeader(2);
        writer.WriteUInt16(id);
        MessagePackSerializer.Serialize(value.GetType(), ref writer, value, options);
    }

    public T? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil()) return null;

        var count = reader.ReadArrayHeader();
        if (count != 2) throw new InvalidDataException();

        var id = reader.ReadUInt16();
        if (!_idToType.TryGetValue(id, out var type))
            throw new InvalidOperationException();

        return (T?)MessagePackSerializer.Deserialize(type, ref reader, options);
    }
}

public class IrNodeResolver : IFormatterResolver
{
    public static readonly IrNodeResolver Instance = new();
    public IMessagePackFormatter<T>? GetFormatter<T>() => FormatterCache<T>.Formatter;

    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter;

        static FormatterCache()
        {
            var type = typeof(T);
            if (typeof(IrNode).IsAssignableFrom(type) && (type.IsAbstract || type.IsInterface))
                Formatter = (IMessagePackFormatter<T>)Activator.CreateInstance(
                    typeof(PolymorphicNodeFormatter<>).MakeGenericType(typeof(T)))!;
        }
    }
}

public static class IrTreeSerializer
{
    private static readonly MessagePackSerializerOptions Options;

    static IrTreeSerializer() =>
        Options = MessagePackSerializerOptions.Standard.WithResolver(
                CompositeResolver.Create([ScratchTypeFormatter.Instance],
                    [IrNodeResolver.Instance, ContractlessStandardResolver.Instance]))
            .WithCompression(MessagePackCompression.Lz4BlockArray);

    public static byte[] Serialize(IrProgramNode root) => MessagePackSerializer.Serialize(root, Options);

    public static IrProgramNode Deserialize(byte[] bytes) =>
        MessagePackSerializer.Deserialize<IrProgramNode>(bytes, Options);
}
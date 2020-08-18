using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public interface ITypeSerializerRegistry
    {
        ITypeSerializer Get(string mediaType);
        bool TryGet(string mediaType, [MaybeNullWhen(false)] out ITypeSerializer typeSerializer);
        ITypeSerializerRegistry Add(string mediaType, ITypeSerializer serializer);
    }
}

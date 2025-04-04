using RootNamespace.Serialization;
using Yardarm.Client.Internal;

namespace RootNamespace.Requests;

public sealed class BuildRequestContext
{
    public ITypeSerializerRegistry TypeSerializerRegistry { get; }

    public BuildRequestContext(ITypeSerializerRegistry typeSerializerRegistry)
    {
        ThrowHelper.ThrowIfNull(typeSerializerRegistry);

        TypeSerializerRegistry = typeSerializerRegistry;
    }
}

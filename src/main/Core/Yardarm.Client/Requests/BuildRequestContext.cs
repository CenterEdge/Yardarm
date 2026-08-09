using System;
using RootNamespace.Serialization;

namespace RootNamespace.Requests;

public sealed class BuildRequestContext
{
    public ITypeSerializerRegistry TypeSerializerRegistry { get; }

    public BuildRequestContext(ITypeSerializerRegistry typeSerializerRegistry)
    {
        ArgumentNullException.ThrowIfNull(typeSerializerRegistry);

        TypeSerializerRegistry = typeSerializerRegistry;
    }
}

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public static class TypeSerializerRegistryExtensions
    {
        public static ITypeSerializerRegistry Add(this ITypeSerializerRegistry typeSerializerRegistry,
            IEnumerable<string> mediaTypes, ITypeSerializer typeSerializer)
        {
            foreach (string mediaType in mediaTypes)
            {
                typeSerializerRegistry = typeSerializerRegistry.Add(mediaType, typeSerializer);
            }

            return typeSerializerRegistry;
        }
    }
}

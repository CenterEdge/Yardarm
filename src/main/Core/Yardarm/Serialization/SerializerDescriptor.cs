using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Serialization
{
    public class SerializerDescriptor
    {
        public IImmutableSet<SerializerMediaType> MediaTypes { get; }

        public string NameSegment { get; }

        public TypeSyntax SerializerType { get; }

        public SerializerDescriptor(IImmutableSet<SerializerMediaType> mediaTypes, string nameSegment, TypeSyntax serializerType)
        {
            ArgumentNullException.ThrowIfNull(mediaTypes);
            ArgumentNullException.ThrowIfNull(nameSegment);
            ArgumentNullException.ThrowIfNull(serializerType);

            MediaTypes = mediaTypes;
            NameSegment = nameSegment;
            SerializerType = serializerType;
        }
    }
}

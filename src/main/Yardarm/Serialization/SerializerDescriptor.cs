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
            MediaTypes = mediaTypes ?? throw new ArgumentNullException(nameof(mediaTypes));
            NameSegment = nameSegment ?? throw new ArgumentNullException(nameof(nameSegment));
            SerializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
        }
    }
}

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

        /// <summary>
        /// If true, the serializer supports deserializing list bodies as <c>IAsyncEnumerable&lt;T&gt;</c> via
        /// <c>DeserializeSequenceAsync</c>, which allows response bodies to be streamed.
        /// </summary>
        public bool SupportsStreaming { get; }

        public SerializerDescriptor(IImmutableSet<SerializerMediaType> mediaTypes, string nameSegment, TypeSyntax serializerType)
            : this(mediaTypes, nameSegment, serializerType, supportsStreaming: false)
        {
        }

        public SerializerDescriptor(IImmutableSet<SerializerMediaType> mediaTypes, string nameSegment, TypeSyntax serializerType,
            bool supportsStreaming)
        {
            ArgumentNullException.ThrowIfNull(mediaTypes);
            ArgumentNullException.ThrowIfNull(nameSegment);
            ArgumentNullException.ThrowIfNull(serializerType);

            MediaTypes = mediaTypes;
            NameSegment = nameSegment;
            SerializerType = serializerType;
            SupportsStreaming = supportsStreaming;
        }
    }
}

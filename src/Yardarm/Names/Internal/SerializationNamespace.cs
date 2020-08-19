using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal
{
    // ReSharper disable InconsistentNaming
    internal class SerializationNamespace : ISerializationNamespace
    {
        public NameSyntax Name { get; }
        public NameSyntax ITypeSerializer { get; }
        public NameSyntax ITypeSerializerRegistry { get; }
        public NameSyntax TypeSerializerRegistryExtensions { get; }

        public SerializationNamespace(IRootNamespace rootNamespace)
        {
            if (rootNamespace == null)
            {
                throw new ArgumentNullException(nameof(rootNamespace));
            }

            Name = QualifiedName(rootNamespace.Name, IdentifierName("Serialization"));

            ITypeSerializer = QualifiedName(
                Name,
                IdentifierName("ITypeSerializer"));

            ITypeSerializerRegistry = QualifiedName(
                Name,
                IdentifierName("ITypeSerializerRegistry"));

            TypeSerializerRegistryExtensions = QualifiedName(
                Name,
                IdentifierName("TypeSerializerRegistryExtensions"));
        }
    }
}

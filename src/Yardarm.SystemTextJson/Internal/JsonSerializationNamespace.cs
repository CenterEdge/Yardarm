using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Internal
{
    internal class JsonSerializationNamespace : IKnownNamespace, IJsonSerializationNamespace
    {
        public NameSyntax Name { get; }
        public NameSyntax JsonTypeSerializer { get; }

        public JsonSerializationNamespace(ISerializationNamespace serializationNamespace)
        {
            if (serializationNamespace == null)
            {
                throw new ArgumentNullException(nameof(serializationNamespace));
            }

            Name = QualifiedName(
                serializationNamespace.Name,
                IdentifierName("Json"));

            JsonTypeSerializer = QualifiedName(
                Name,
                IdentifierName("JsonTypeSerializer"));
        }

        public TypeSyntax JsonStringEnumConverter(TypeSyntax valueType) =>
            QualifiedName(
                Name,
                GenericName(
                    Identifier("JsonStringEnumConverter"),
                    TypeArgumentList(SingletonSeparatedList(valueType))));
    }
}

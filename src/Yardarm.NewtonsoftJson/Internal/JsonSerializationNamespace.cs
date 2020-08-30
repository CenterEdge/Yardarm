using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson.Internal
{
    internal class JsonSerializationNamespace : IKnownNamespace, IJsonSerializationNamespace
    {
        public NameSyntax Name { get; }
        public NameSyntax DiscriminatorConverter { get; }
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

            DiscriminatorConverter = QualifiedName(
                Name,
                IdentifierName("DiscriminatorConverter"));

            JsonTypeSerializer = QualifiedName(
                Name,
                IdentifierName("JsonTypeSerializer"));
        }

        public TypeSyntax AdditionalPropertiesDictionary(TypeSyntax valueType) =>
            QualifiedName(
                Name,
                GenericName(
                    Identifier("AdditionalPropertiesDictionary"),
                    TypeArgumentList(SingletonSeparatedList(valueType))));

    }
}

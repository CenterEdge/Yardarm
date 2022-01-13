using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Internal
{
    internal class JsonSerializationNamespace : IKnownNamespace, IJsonSerializationNamespace
    {
        public NameSyntax Name { get; }
        public NameSyntax JsonTypeSerializer { get; }
        public NameSyntax JsonHelpers { get; }

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

            JsonHelpers = QualifiedName(
                Name,
                IdentifierName("JsonHelpers"));
        }

        public TypeSyntax JsonStringEnumConverter(TypeSyntax valueType) =>
            QualifiedName(
                Name,
                GenericName(
                    Identifier("JsonStringEnumConverter"),
                    TypeArgumentList(SingletonSeparatedList(valueType))));

        public InvocationExpressionSyntax GetDiscriminator(ExpressionSyntax reader, ExpressionSyntax utf8PropertyName) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    JsonHelpers,
                    IdentifierName("GetDiscriminator")),
                ArgumentList(SeparatedList<ArgumentSyntax>(new[] {
                    Argument(null, Token(SyntaxKind.RefKeyword), reader),
                    Argument(utf8PropertyName) })));
    }
}

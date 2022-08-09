using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class SerializationDataPropertyGenerator : IRequestMemberGenerator
    {
        public const string SerializationDataPropertyName = "SerializationData";

        protected ISerializationNamespace SerializationNamespace { get; }

        public SerializationDataPropertyGenerator(ISerializationNamespace serializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(serializationNamespace);

            SerializationNamespace = serializationNamespace;
        }

        public IEnumerable<MemberDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType)
        {
            if (mediaType == null)
            {
                // Only add the property in inherited classes for a specific media type
                yield break;
            }

            // TODO: Add handling for specific media types

            // Default behavior is to return null
            yield return PropertyDeclaration(
                default,
                TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)),
                NullableType(SerializationNamespace.ISerializationData),
                null,
                Identifier(SerializationDataPropertyName),
                null,
                ArrowExpressionClause(LiteralExpression(SyntaxKind.NullLiteralExpression)),
                null,
                Token(SyntaxKind.SemicolonToken));
        }

        // If type is null, assumes we're accessing from within the same class
        public static ExpressionSyntax GetSerializationData(TypeSyntax? type = null) =>
            type is not null
                ? MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    type,
                    IdentifierName(SerializationDataPropertyName))
                : IdentifierName(SerializationDataPropertyName);
    }
}

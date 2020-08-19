using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class BuildContentMethodGenerator : IBuildContentMethodGenerator
    {
        public const string BuildContentMethodName = "BuildContent";

        private const string TypeSerializerRegistryParameterName = "typeSerializerRegistry";

        protected ISerializationNamespace SerializationNamespace { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }

        public BuildContentMethodGenerator(ISerializationNamespace serializationNamespace,
            IMediaTypeSelector mediaTypeSelector)
        {
            SerializationNamespace =
                serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public MethodDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodDeclaration(
                    NullableType(WellKnownTypes.System.Net.Http.HttpContent.Name),
                    BuildContentMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier(TypeSerializerRegistryParameterName))
                        .WithType(SerializationNamespace.ITypeSerializerRegistry))
                .WithBody(Block(GenerateStatements(operation)));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            var requestBody = operation.GetRequestBody();

            ILocatedOpenApiElement<OpenApiMediaType>? mediaType;
            if (requestBody == null || (mediaType = MediaTypeSelector.Select(requestBody)) == null)
            {
                yield return ReturnStatement(LiteralExpression(SyntaxKind.NullLiteralExpression));
                yield break;
            }

            var createContentExpression =
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.TypeSerializerRegistryExtensions,
                        IdentifierName("Serialize")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName(TypeSerializerRegistryParameterName)),
                        Argument(IdentifierName(RequestTypeGenerator.BodyPropertyName)),
                        Argument(SyntaxHelpers.StringLiteral(mediaType.Key)));

            yield return ReturnStatement(ConditionalExpression(
                IsPatternExpression(
                    IdentifierName(RequestTypeGenerator.BodyPropertyName),
                    ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                LiteralExpression(SyntaxKind.NullLiteralExpression),
                createContentExpression));
        }

        public static InvocationExpressionSyntax InvokeBuildContent(ExpressionSyntax requestInstance,
            ExpressionSyntax typeSerializerRegistry) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    requestInstance,
                    IdentifierName(BuildContentMethodName)))
                .AddArgumentListArguments(
                    Argument(typeSerializerRegistry));
    }
}

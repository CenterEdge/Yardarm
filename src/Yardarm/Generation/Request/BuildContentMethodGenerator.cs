using System;
using System.Collections.Generic;
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

        public MethodDeclarationSyntax GenerateHeader(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodDeclaration(
                    NullableType(WellKnownTypes.System.Net.Http.HttpContent.Name),
                    BuildContentMethodName)
                .AddParameterListParameters(
                    Parameter(Identifier(TypeSerializerRegistryParameterName))
                        .WithType(SerializationNamespace.ITypeSerializerRegistry));

        public MethodDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType)
        {
            MethodDeclarationSyntax methodDeclaration = GenerateHeader(operation);

            if (mediaType == null)
            {
                // In the base request class which has no body
                methodDeclaration = methodDeclaration
                    .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword))
                    .WithExpressionBody(ArrowExpressionClause(
                        LiteralExpression(SyntaxKind.NullLiteralExpression)));
            }
            else
            {
                // In an inherited request class which adds a body
                methodDeclaration = methodDeclaration
                    .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
                    .WithBody(Block(GenerateStatements(operation, mediaType)));
            }

            return methodDeclaration;
        }

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            ILocatedOpenApiElement<OpenApiOperation> operation, ILocatedOpenApiElement<OpenApiMediaType> mediaType)
        {
            var createContentExpression =
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.TypeSerializerRegistryExtensions,
                        IdentifierName("Serialize")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName(TypeSerializerRegistryParameterName)),
                        Argument(IdentifierName(RequestMediaTypeGenerator.BodyPropertyName)),
                        Argument(SyntaxHelpers.StringLiteral(mediaType.Key)));

            yield return ReturnStatement(ConditionalExpression(
                IsPatternExpression(
                    IdentifierName(RequestMediaTypeGenerator.BodyPropertyName),
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

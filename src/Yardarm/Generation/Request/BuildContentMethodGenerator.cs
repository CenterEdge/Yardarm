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

        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected INameFormatterSelector NameFormatterSelector { get; }

        public BuildContentMethodGenerator(IMediaTypeSelector mediaTypeSelector, INameFormatterSelector nameFormatterSelector)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            NameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
        }

        public MethodDeclarationSyntax Generate(LocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodDeclaration(
                    NullableType(WellKnownTypes.System.Net.Http.HttpContent.Name),
                    BuildContentMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithBody(Block(GenerateStatements(operation)));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            LocatedOpenApiElement<OpenApiOperation> operation)
        {
            var requestBody = operation.GetRequestBody();

            LocatedOpenApiElement<OpenApiMediaType>? mediaType;
            if (requestBody == null || (mediaType = MediaTypeSelector.Select(requestBody)) == null)
            {
                yield return ReturnStatement(LiteralExpression(SyntaxKind.NullLiteralExpression));
                yield break;
            }

            var createContentExpression =
                ObjectCreationExpression(WellKnownTypes.System.Net.Http.StringContent.Name)
                    .AddArgumentListArguments(
                        Argument(SyntaxHelpers.StringLiteral("")),
                        Argument(SyntaxHelpers.MemberAccess("System", "Text", "Encoding", "UTF8")),
                        Argument(SyntaxHelpers.StringLiteral(mediaType.Key)));

            yield return ReturnStatement(ConditionalExpression(
                IsPatternExpression(
                    IdentifierName(RequestTypeGenerator.BodyPropertyName),
                    ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                LiteralExpression(SyntaxKind.NullLiteralExpression),
                createContentExpression));
        }

        public static InvocationExpressionSyntax InvokeBuildContent(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    requestInstance,
                    IdentifierName(BuildContentMethodName)));
    }
}

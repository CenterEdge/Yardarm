using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    /// <summary>
    /// Generates constructors which accept headers, primarily used for mocking responses for unit tests.
    /// </summary>
    internal class NoBodyConstructorMethodGenerator : IResponseMethodGenerator
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;

        public NoBodyConstructorMethodGenerator(GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            _context = context;
            _mediaTypeSelector = mediaTypeSelector;
        }

        public IEnumerable<BaseMethodDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiResponse> response, string className)
        {
            var bodyType = GetBodyType(response);
            if (bodyType is not null)
            {
                // We don't want these overloads if there is a body, they'll just add confusion
                yield break;
            }

            if (!response.IsRoot())
            {
                // Construct from status code and headers without body
                yield return ConstructorDeclaration(
                    default,
                    new SyntaxTokenList(Token(SyntaxKind.PublicKeyword)),
                    Identifier(className),
                    ParameterList(SingletonSeparatedList(
                        Parameter(
                            default,
                            default,
                            NullableType(WellKnownTypes.System.Net.Http.Headers.HttpResponseHeaders.Name),
                            Identifier("headers"),
                            EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression))))),
                    ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                        ArgumentList(SeparatedList(new[]
                        {
                            Argument(CastExpression(
                                WellKnownTypes.System.Net.HttpStatusCode.Name,
                                LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                    Literal(response.Key, int.Parse(response.Key))))),
                            Argument(IdentifierName("headers"))
                        }))),
                    Block());
            }
            else
            {
                // Construct from passed status code and headers without body, inherited types will supply the status code
                yield return ConstructorDeclaration(
                    default,
                    new SyntaxTokenList(Token(SyntaxKind.ProtectedKeyword)),
                    Identifier(className),
                    ParameterList(SeparatedList(new []
                    {
                        Parameter(
                            default,
                            default,
                            WellKnownTypes.System.Net.HttpStatusCode.Name,
                            Identifier("statusCode"),
                            null),
                        Parameter(
                            default,
                            default,
                            NullableType(WellKnownTypes.System.Net.Http.Headers.HttpResponseHeaders.Name),
                            Identifier("headers"),
                            EqualsValueClause(LiteralExpression(SyntaxKind.NullLiteralExpression)))
                    })),
                    ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                        ArgumentList(SeparatedList(new[]
                        {
                            Argument(IdentifierName("statusCode")),
                            Argument(IdentifierName("headers"))
                        }))),
                    Block());
            }
        }

        private TypeSyntax? GetBodyType(ILocatedOpenApiElement<OpenApiResponse> response)
        {
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = _mediaTypeSelector.Select(response);
            if (mediaType == null)
            {
                return null;
            }

            ILocatedOpenApiElement<OpenApiSchema>? schemaElement = mediaType.GetSchema();
            if (schemaElement == null)
            {
                return null;
            }

            return _context.TypeGeneratorRegistry.Get(schemaElement).TypeInfo.Name;
        }
    }
}

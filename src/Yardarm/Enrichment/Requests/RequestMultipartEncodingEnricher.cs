using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Request;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Requests
{
    public class RequestMultipartEncodingEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiMediaType>
    {
        // Default encodings when the spec doesn't specify the encoding
        private static readonly ArgumentSyntax[] PlainTextEncoding =
        {
            Argument(SyntaxHelpers.StringLiteral("text/plain"))
        };

        private static readonly ArgumentSyntax[] JsonEncoding =
        {
            Argument(SyntaxHelpers.StringLiteral("application/json"))
        };

        private static readonly ArgumentSyntax[] OctetStreamEncoding =
        {
            Argument(SyntaxHelpers.StringLiteral("application/octet-stream"))
        };

        private readonly ISerializationNamespace _serializationNamespace;
        private readonly ITypeGeneratorRegistry<OpenApiSchema> _schemaRegistry;
        private readonly INameFormatter _propertyNameFormatter;

        public RequestMultipartEncodingEnricher(ISerializationNamespace serializationNamespace,
            ITypeGeneratorRegistry<OpenApiSchema> schemaRegistry,
            INameFormatterSelector nameFormatterSelector)
        {
            ArgumentNullException.ThrowIfNull(serializationNamespace);
            ArgumentNullException.ThrowIfNull(schemaRegistry);
            ArgumentNullException.ThrowIfNull(nameFormatterSelector);

            _serializationNamespace = serializationNamespace;
            _schemaRegistry = schemaRegistry;
            _propertyNameFormatter = nameFormatterSelector.GetFormatter(NameKind.Property);
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiMediaType> context) =>
            IsMultipartEncoding(context.LocatedElement.Key)
            && target.GetGeneratorAnnotation() == typeof(RequestMediaTypeGenerator)
                ? AddSerializationData(target, context)
                : target;

        private ClassDeclarationSyntax AddSerializationData(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiMediaType> element)
        {
            var serializationDataProperty = target.Members
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p =>
                    p.Identifier.ValueText == SerializationDataPropertyGenerator.SerializationDataPropertyName);

            if (serializationDataProperty is null)
            {
                return target;
            }

            var schemaType = _schemaRegistry.Get(element.LocatedElement.GetSchemaOrDefault())
                .TypeInfo.Name;

            // Build an initializer
            var initializer = ObjectCreationExpression(
                _serializationNamespace.MultipartFormDataSerializationData(schemaType),
                ArgumentList(SeparatedList(
                    GetProperties(element.Element).Select(p =>
                    {
                        element.Element.Encoding.TryGetValue(p.Key, out OpenApiEncoding? encoding);

                        return CreateArgument(schemaType, p.Key, p.Value, encoding)
                            .WithLeadingTrivia(ElasticCarriageReturnLineFeed);
                    }))),
                null);

            // Replace the accessors or arrow function with a simple get accessor and an initializer
            var newProperty = serializationDataProperty.Update(
                serializationDataProperty.AttributeLists,
                serializationDataProperty.Modifiers,
                serializationDataProperty.Type,
                serializationDataProperty.ExplicitInterfaceSpecifier,
                serializationDataProperty.Identifier,
                AccessorList(SingletonList(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, default, default,
                        Token(SyntaxKind.GetKeyword), null, null, Token(SyntaxKind.SemicolonToken)))),
                null,
                EqualsValueClause(initializer),
                Token(SyntaxKind.SemicolonToken));

            return target.ReplaceNode(serializationDataProperty, newProperty);
        }

        private ArgumentSyntax CreateArgument(TypeSyntax classIdentifier, string propertyName,
            OpenApiSchema propertySchema, OpenApiEncoding? encoding)
        {
            var mediaTypes = encoding?.ContentType != null
                ? GetMediaTypes(encoding.ContentType)
                    .Select(p => Argument(SyntaxHelpers.StringLiteral(p)))
                    .ToArray()
                : null;

            if (mediaTypes == null || mediaTypes.Length == 0)
            {
                mediaTypes = SelectDefaultMediaTypes(propertySchema);
            }

            var arguments = new[]
            {
                Argument(SimpleLambdaExpression(
                    Parameter(Identifier("p")),
                    null,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("p"),
                        IdentifierName(_propertyNameFormatter.Format(propertyName))))),
                Argument(SyntaxHelpers.StringLiteral(propertyName))
            }.Concat(mediaTypes);

            return Argument(InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    _serializationNamespace.MultipartPropertyInfo(classIdentifier),
                    IdentifierName("Create")),
                ArgumentList(SeparatedList(arguments))));
        }

        private static ArgumentSyntax[] SelectDefaultMediaTypes(OpenApiSchema schema) =>
            schema switch
            {
                {Type: "string", Format: "binary" or "base64"} => OctetStreamEncoding,
                {Type: "object"} or {Type: "array", Items.Type: "object"} => JsonEncoding,
                _ => PlainTextEncoding
            };

        private static IEnumerable<KeyValuePair<string, OpenApiSchema>> GetProperties(OpenApiMediaType element) =>
            element.Schema?.Properties ?? Enumerable.Empty<KeyValuePair<string, OpenApiSchema>>();

        private static IEnumerable<string> GetMediaTypes(string contentType) =>
            contentType.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        private static bool IsMultipartEncoding(string contentType) =>
            contentType == "application/x-www-form-urlencoded" || contentType.StartsWith("multipart/");
    }
}

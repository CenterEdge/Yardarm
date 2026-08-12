using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Spec;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Request;
using Yardarm.Helpers;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Requests
{
    public class RequestMultipartEncodingEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, IOpenApiMediaType>
    {
        public Type[] ExecuteBefore { get; } =
        {
            // Make sure this executes before the Body property is marked as nullable
            typeof(RequiredPropertyEnricher)
        };

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
        private readonly ITypeGeneratorRegistry<IOpenApiSchema> _schemaRegistry;
        private readonly INameFormatter _propertyNameFormatter;

        public RequestMultipartEncodingEnricher(ISerializationNamespace serializationNamespace,
            ITypeGeneratorRegistry<IOpenApiSchema> schemaRegistry,
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
            OpenApiEnrichmentContext<IOpenApiMediaType> context) =>
            IsMultipartEncoding(context.LocatedElement.Key)
            && target.GetGeneratorAnnotation() == typeof(RequestMediaTypeGenerator)
                ? AddSerializationData(AddBodyClass(target, context, out TypeSyntax? bodyType), context, bodyType)
                : target;

        private ClassDeclarationSyntax AddBodyClass(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiMediaType> context,
            out TypeSyntax? bodyType)
        {
            var bodyProperty = target.Members
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == RequestMediaTypeGenerator.BodyPropertyName);
            if (bodyProperty is null)
            {
                bodyType = null;
                return target;
            }

            var bodyClass = ClassDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword)),
                Identifier("MultipartBody"),
                null,
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(bodyProperty.Type))),
                default,
                List(GetProperties(context.Element).Select(property =>
                {
                    return (MemberDeclarationSyntax)PropertyDeclaration(
                            default,
                            TokenList(Token(SyntaxKind.PublicKeyword)),
                            _serializationNamespace.MultipartFieldDetails.MakeNullable(),
                            null,
                            Identifier(_propertyNameFormatter.Format(property.Key + "-Details")),
                            AccessorList(List(new[]
                            {
                                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                            })),
                            null,
                            EqualsValueClause(ImplicitObjectCreationExpression()),
                            Token(SyntaxKind.SemicolonToken))
                        .WithLeadingTrivia(
                            DocumentationSyntaxHelpers.BuildXmlCommentTrivia(XmlSummaryElement(
                                DocumentationSyntaxHelpers.InteriorNewLine(),
                                XmlText(XmlTextLiteral("Optionally supplies additional details for ")),
                                XmlSeeElement(NameMemberCref(IdentifierName(_propertyNameFormatter.Format(property.Key)))),
                                XmlText(XmlTextLiteral(".")),
                                DocumentationSyntaxHelpers.InteriorNewLine())));
                })));

            bodyType = IdentifierName(bodyClass.Identifier);
            return target
                .ReplaceNode(bodyProperty, bodyProperty.WithType(bodyType))
                .AddMembers(bodyClass);
        }

        private ClassDeclarationSyntax AddSerializationData(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiMediaType> element, TypeSyntax? bodyType)
        {
            if (bodyType is null)
            {
                return target;
            }

            var serializationDataProperty = target.Members
                .OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p =>
                    p.Identifier.ValueText == SerializationDataPropertyGenerator.SerializationDataPropertyName);

            if (serializationDataProperty is null)
            {
                return target;
            }

            // Build an initializer
            var initializer = ObjectCreationExpression(
                _serializationNamespace.MultipartFormDataSerializationData(bodyType),
                ArgumentList(SeparatedList(
                    GetProperties(element.Element).Select(p =>
                    {
                        element.Element.Encoding.TryGetValue(p.Key, out OpenApiEncoding? encoding);

                        return CreateArgument(bodyType, p.Key, p.Value, encoding)
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
            IOpenApiSchema propertySchema, OpenApiEncoding? encoding)
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
                Argument(SimpleLambdaExpression(
                    Parameter(Identifier("p")),
                    null,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("p"),
                        IdentifierName(_propertyNameFormatter.Format(propertyName + "-Details"))))),
                Argument(SyntaxHelpers.StringLiteral(propertyName))
            }.Concat(mediaTypes);

            return Argument(InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    _serializationNamespace.MultipartPropertyInfo(classIdentifier),
                    IdentifierName("Create")),
                ArgumentList(SeparatedList(arguments))));
        }

        private static ArgumentSyntax[] SelectDefaultMediaTypes(IOpenApiSchema schema) =>
            schema switch
            {
                _ when schema.HasType(JsonSchemaType.String) && schema.Format is "binary" or "base64" => OctetStreamEncoding,
                _ when schema.HasType(JsonSchemaType.Object) => JsonEncoding,
                _ when schema.HasType(JsonSchemaType.Array) && schema.Items is not null && schema.Items.HasType(JsonSchemaType.Object) => JsonEncoding,
                _ => PlainTextEncoding
            };

        private static IEnumerable<KeyValuePair<string, IOpenApiSchema>> GetProperties(IOpenApiMediaType element) =>
            element.Schema?.Properties ?? Enumerable.Empty<KeyValuePair<string, IOpenApiSchema>>();

        private static IEnumerable<string> GetMediaTypes(string contentType) =>
            contentType.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        private static bool IsMultipartEncoding(string contentType) =>
            contentType == "application/x-www-form-urlencoded" || contentType.StartsWith("multipart/");
    }
}

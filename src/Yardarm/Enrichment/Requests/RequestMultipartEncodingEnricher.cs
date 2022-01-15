using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Requests
{
    public class RequestMultipartEncodingEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiMediaType>
    {
        // Default encodings when the spec doesn't specify the encoding
        private static readonly AttributeArgumentSyntax[] PlainTextEncoding =
        {
            AttributeArgument(SyntaxHelpers.StringLiteral("text/plain"))
        };

        private static readonly AttributeArgumentSyntax[] JsonEncoding =
        {
            AttributeArgument(SyntaxHelpers.StringLiteral("application/json"))
        };

        private static readonly AttributeArgumentSyntax[] OctetStreamEncoding =
        {
            AttributeArgument(SyntaxHelpers.StringLiteral("application/octet-stream"))
        };

        private readonly ISerializationNamespace _serializationNamespace;

        public RequestMultipartEncodingEnricher(ISerializationNamespace serializationNamespace)
        {
            _serializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiMediaType> context) =>
            IsMultipartEncoding(context.LocatedElement.Key)
            && target.GetGeneratorAnnotation() == typeof(RequestMediaTypeGenerator)
                ? AddEncodingAttributes(target, context.Element)
                : target;

        private ClassDeclarationSyntax AddEncodingAttributes(ClassDeclarationSyntax target,
            OpenApiMediaType element) =>
            target.AddAttributeLists(AttributeList(SeparatedList(
                GetProperties(element)
                    .Select(p =>
                    {
                        element.Encoding.TryGetValue(p.Key, out OpenApiEncoding? encoding);

                        return CreateAttribute(p.Key, p.Value, encoding);
                    }))));

        private AttributeSyntax CreateAttribute(string propertyName, OpenApiSchema propertySchema, OpenApiEncoding? encoding)
        {
            var mediaTypes = encoding?.ContentType != null
                ? GetMediaTypes(encoding.ContentType)
                    .Select(p => AttributeArgument(SyntaxHelpers.StringLiteral(p)))
                    .ToArray()
                : null;

            if (mediaTypes == null || mediaTypes.Length == 0)
            {
                mediaTypes = SelectDefaultMediaTypes(propertySchema);
            }

            return Attribute(_serializationNamespace.MultipartEncodingAttribute)
                .AddArgumentListArguments(AttributeArgument(SyntaxHelpers.StringLiteral(propertyName)))
                .AddArgumentListArguments(mediaTypes);
        }

        private static AttributeArgumentSyntax[] SelectDefaultMediaTypes(OpenApiSchema schema) =>
            schema switch
            {
                {Type: "string", Format: "binary" or "base64"} => OctetStreamEncoding,
                {Type: "object"} or {Type: "array", Items.Type: "object"} => JsonEncoding,
                _ => PlainTextEncoding
            };

        private static IEnumerable<KeyValuePair<string, OpenApiSchema>> GetProperties(OpenApiMediaType element) =>
            element.Schema?.Properties ?? Enumerable.Empty<KeyValuePair<string, OpenApiSchema>>();

        private static IEnumerable<string> GetMediaTypes(string contentType) =>
            contentType.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());

        private static bool IsMultipartEncoding(string contentType) =>
            contentType == "application/x-www-form-urlencoded" || contentType.StartsWith("multipart/");
    }
}

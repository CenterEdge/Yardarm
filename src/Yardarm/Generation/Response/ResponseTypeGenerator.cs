using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    internal class ResponseTypeGenerator : TypeGeneratorBase<OpenApiResponse>
    {
        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected IHttpResponseCodeNameProvider HttpResponseCodeNameProvider { get; }

        protected OpenApiResponse Response => Element.Element;

        public ResponseTypeGenerator(LocatedOpenApiElement<OpenApiResponse> responseElement, GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
            : base(responseElement, context)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            HttpResponseCodeNameProvider = httpResponseCodeNameProvider ?? throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

        public override void Preprocess()
        {
            (ITypeGenerator? schemaGenerator, bool schemaIsReference) = GetSchemaGenerator();

            if (schemaGenerator != null && !schemaIsReference)
            {
                schemaGenerator.Preprocess();
            }
        }

        public override TypeSyntax GetTypeName()
        {
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return QualifiedName(ns, IdentifierName(GetClassName()));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            (ITypeGenerator? schemaGenerator, bool schemaIsReference) = GetSchemaGenerator();

            var declaration = ClassDeclaration(GetClassName())
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(GenerateHeaderProperties().ToArray());

            if (schemaGenerator != null)
            {
                declaration = declaration
                    .AddMembers(PropertyDeclaration(schemaGenerator.GetTypeName(), Identifier("Body"))
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                            AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))));

                if (!schemaIsReference)
                {
                    declaration = declaration.AddMembers(schemaGenerator.Generate().ToArray());
                }
            }

            yield return declaration;
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateHeaderProperties()
        {
            var nameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            foreach (var header in Response.Headers.Select(p => Element.CreateChild(p.Value, p.Key)))
            {
                var schemaElement = header.CreateChild(header.Element.Schema, "Header");

                ITypeGenerator schemaGenerator = Context.SchemaGeneratorRegistry.Get(schemaElement);

                yield return PropertyDeclaration(schemaGenerator.GetTypeName(), nameFormatter.Format(header.Key))
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

                if (schemaElement.Element.Reference == null)
                {
                    foreach (var memberDeclaration in schemaGenerator.Generate())
                    {
                        yield return memberDeclaration;
                    }
                }
            }
        }

        private (ITypeGenerator? schemaGenerator, bool isReference) GetSchemaGenerator()
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(Element);
            if (mediaType == null)
            {
                return (null, false);
            }

            var schemaElement = mediaType.CreateChild(mediaType.Element.Schema, "Body");
            return (Context.SchemaGeneratorRegistry.Get(schemaElement), schemaElement.Element.Reference != null);
        }

        private string GetClassName()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);

            if (Response.Reference != null)
            {
                // We're in the components section

                return formatter.Format(Response.Reference.Id + "Response");
            }
            else
            {
                // We're in an operation

                var operation = Element.Parents.OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

                var responseCode = Enum.TryParse<HttpStatusCode>(Element.Key, out var statusCode)
                    ? HttpResponseCodeNameProvider.GetName(statusCode)
                    : Element.Key;

                return formatter.Format($"{operation.OperationId}{responseCode}Response");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    internal class ResponseTypeGenerator : TypeGeneratorBase<OpenApiResponse>
    {
        private static readonly LocatedOpenApiElement<OpenApiSchema> _defaultHeaderSchema =
            new LocatedOpenApiElement<OpenApiSchema>(new OpenApiSchema
            {
                Type = "string"
            }, "");

        protected ResponseBaseTypeGenerator ResponseBaseTypeGenerator { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected IHttpResponseCodeNameProvider HttpResponseCodeNameProvider { get; }
        protected IGetBodyMethodGenerator ParseBodyMethodGenerator { get; }

        protected OpenApiResponse Response => Element.Element;

        public ResponseTypeGenerator(LocatedOpenApiElement<OpenApiResponse> responseElement, GenerationContext context,
            IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider,
            ResponseBaseTypeGenerator responseBaseTypeGenerator,
            IGetBodyMethodGenerator parseBodyMethodGenerator)
            : base(responseElement, context)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            HttpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                           throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
            ResponseBaseTypeGenerator = responseBaseTypeGenerator ?? throw new ArgumentNullException(nameof(responseBaseTypeGenerator));
            ParseBodyMethodGenerator = parseBodyMethodGenerator ?? throw new ArgumentNullException(nameof(parseBodyMethodGenerator));
        }

        protected override TypeSyntax GetTypeName()
        {
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return QualifiedName(ns, IdentifierName(GetClassName()));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var className = GetClassName();

            var declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(ResponseBaseTypeGenerator.TypeName))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    new MemberDeclarationSyntax?[]
                        {
                            GenerateConstructor(className),
                            ParseBodyMethodGenerator.Generate(Element)
                        }
                        .Concat(GenerateHeaderProperties())
                        .Where(p => p != null)
                        .ToArray()!);

            (ITypeGenerator? schemaGenerator, bool schemaIsReference) = GetSchemaGenerator();
            if (schemaGenerator != null && !schemaIsReference)
            {
                declaration = declaration.AddMembers(schemaGenerator.Generate().ToArray());
            }

            yield return declaration;
        }

        private ConstructorDeclarationSyntax GenerateConstructor(string className) =>
            ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("message"))
                        .WithType(WellKnownTypes.System.Net.Http.HttpResponseMessage.Name),
                    Parameter(Identifier("typeSerializerRegistry"))
                        .WithType(Context.NamespaceProvider.GetITypeSerializerRegistry()))
                .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(
                        Argument(IdentifierName("message")),
                        Argument(IdentifierName("typeSerializerRegistry"))))
                .WithBody(Block());

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateHeaderProperties()
        {
            var nameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            foreach (var header in Response.Headers.Select(p => Element.CreateChild(p.Value, p.Key)))
            {
                LocatedOpenApiElement<OpenApiSchema> schemaElement = header.Element.Schema != null
                    ? header.CreateChild(header.Element.Schema, "Header")
                    : _defaultHeaderSchema;

                ITypeGenerator schemaGenerator = Context.SchemaGeneratorRegistry.Get(schemaElement);

                yield return PropertyDeclaration(schemaGenerator.TypeName, nameFormatter.Format(header.Key))
                    .AddElementAnnotation(header, Context.ElementRegistry)
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

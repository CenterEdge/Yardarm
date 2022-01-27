using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
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
        protected IResponsesNamespace ResponsesNamespace { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected IHttpResponseCodeNameProvider HttpResponseCodeNameProvider { get; }
        protected ISerializationNamespace SerializationNamespace { get; }
        protected IGetBodyMethodGenerator GetBodyMethodGenerator { get; }

        protected OpenApiResponse Response => Element.Element;

        public ResponseTypeGenerator(ILocatedOpenApiElement<OpenApiResponse> responseElement, GenerationContext context,
            IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider,
            ISerializationNamespace serializationNamespace,
            IResponsesNamespace responsesNamespace,
            IGetBodyMethodGenerator getBodyMethodGenerator)
            : base(responseElement, context, null)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            HttpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                           throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
            SerializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
            ResponsesNamespace = responsesNamespace ?? throw new ArgumentNullException(nameof(responsesNamespace));
            GetBodyMethodGenerator = getBodyMethodGenerator ?? throw new ArgumentNullException(nameof(getBodyMethodGenerator));
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return new YardarmTypeInfo(QualifiedName(ns, IdentifierName(GetClassName())));
        }

        public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child,
            NameKind nameKind) =>
            QualifiedName(
                (QualifiedNameSyntax)TypeInfo.Name,
                IdentifierName(Context.NameFormatterSelector.GetFormatter(nameKind).Format(child.Key + "-Model")));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = GetClassName();

            bool isPrimaryImplementation = Element.IsRoot() || Response.Reference == null;

            // For non-primary implementations (referencing a response in the components section),
            // inherit from the primary implementation
            TypeSyntax baseType = isPrimaryImplementation
                ? ResponsesNamespace.OperationResponse
                : Context.TypeGeneratorRegistry.Get(
                    Context.Document.ResolveComponentReference<OpenApiResponse>(Response.Reference!)).TypeInfo.Name;

            var declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(baseType))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(GenerateConstructors(className).ToArray<MemberDeclarationSyntax>());

            if (isPrimaryImplementation)
            {
                declaration = declaration.AddMembers(
                    new MemberDeclarationSyntax?[]
                        {
                            isPrimaryImplementation ? GetBodyMethodGenerator.Generate(Element) : null
                        }
                        .Concat(GenerateHeaderProperties())
                        .Where(p => p != null)
                        .ToArray()!);
            }

            (ITypeGenerator? schemaGenerator, bool schemaIsReference) = GetSchemaGenerator();
            if (schemaGenerator != null && !schemaIsReference)
            {
                declaration = declaration.AddMembers(schemaGenerator.Generate().ToArray());
            }

            yield return declaration;
        }

        private IEnumerable<ConstructorDeclarationSyntax> GenerateConstructors(string className)
        {
            yield return ConstructorDeclaration(
                default,
                new SyntaxTokenList(Token(SyntaxKind.PublicKeyword)),
                Identifier(className),
                ParameterList(SingletonSeparatedList(
                    Parameter(
                        default,
                        default,
                        WellKnownTypes.System.Net.Http.HttpResponseMessage.Name,
                        Identifier("message"),
                        null))),
                ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    ArgumentList(SingletonSeparatedList(
                        Argument(IdentifierName("message"))))),
                Block());

            yield return ConstructorDeclaration(
                default,
                new SyntaxTokenList(Token(SyntaxKind.PublicKeyword)),
                Identifier(className),
                ParameterList(SeparatedList(new[] {
                    Parameter(
                        default,
                        default,
                        WellKnownTypes.System.Net.Http.HttpResponseMessage.Name,
                        Identifier("message"),
                        null),
                    Parameter(
                        default,
                        default,
                        SerializationNamespace.ITypeSerializerRegistry,
                        Identifier("typeSerializerRegistry"),
                        null)
                })),
                ConstructorInitializer(SyntaxKind.BaseConstructorInitializer,
                    ArgumentList(SeparatedList(new[] {
                        Argument(IdentifierName("message")),
                        Argument(IdentifierName("typeSerializerRegistry"))
                    }))),
                Block());
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateHeaderProperties()
        {
            var nameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            foreach (var header in Element.GetHeaders())
            {
                var headerGenerator = Context.TypeGeneratorRegistry.Get(header);

                yield return PropertyDeclaration(headerGenerator.TypeInfo.Name, nameFormatter.Format(header.Key))
                    .AddElementAnnotation(header, Context.ElementRegistry)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

                if (!header.IsReference())
                {
                    ILocatedOpenApiElement<OpenApiSchema> schemaElement = header.GetSchemaOrDefault();
                    if (!schemaElement.IsReference())
                    {
                        foreach (var memberDeclaration in headerGenerator.Generate())
                        {
                            yield return memberDeclaration;
                        }
                    }
                }
            }
        }

        private (ITypeGenerator? schemaGenerator, bool isReference) GetSchemaGenerator()
        {
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(Element);
            if (mediaType == null)
            {
                return (null, false);
            }

            ILocatedOpenApiElement<OpenApiSchema>? schemaElement = mediaType.GetSchema();
            if (schemaElement == null)
            {
                return (null, false);
            }

            return (Context.TypeGeneratorRegistry.Get(schemaElement), schemaElement.Element.Reference != null);
        }

        private string GetClassName()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);

            if (Element.IsRoot())
            {
                // We're in the components section

                return formatter.Format(Response.Reference.Id + "Response");
            }
            else
            {
                // We're in an operation

                OpenApiOperation operation = Element.Parents()
                    .OfType<LocatedOpenApiElement<OpenApiOperation>>()
                    .First()
                    .Element;

                string responseCode = Enum.TryParse<HttpStatusCode>(Element.Key, out var statusCode)
                    ? HttpResponseCodeNameProvider.GetName(statusCode)
                    : Element.Key;

                return formatter.Format($"{operation.OperationId}{responseCode}Response");
            }
        }
    }
}

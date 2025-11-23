using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Operation;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    internal class ResponseTypeGenerator(
        ILocatedOpenApiElement<OpenApiResponse> responseElement,
        GenerationContext context,
        IMediaTypeSelector mediaTypeSelector,
        IHttpResponseCodeNameProvider httpResponseCodeNameProvider,
        ISerializationNamespace serializationNamespace,
        IResponsesNamespace responsesNamespace,
        IEnumerable<IResponseMethodGenerator> methodGenerators,
        IOperationNameProvider operationNameProvider)
        : TypeGeneratorBase<OpenApiResponse>(responseElement, context, null)
    {
        public const string BodyFieldName = "_body";

        protected IResponsesNamespace ResponsesNamespace { get; } = responsesNamespace;
        protected IMediaTypeSelector MediaTypeSelector { get; } = mediaTypeSelector;
        protected IHttpResponseCodeNameProvider HttpResponseCodeNameProvider { get; } = httpResponseCodeNameProvider;
        protected ISerializationNamespace SerializationNamespace { get; } = serializationNamespace;
        protected IResponseMethodGenerator[] MethodGenerators { get; } = methodGenerators.ToArray();

        protected OpenApiResponse Response => Element.Element;

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

            bool isPrimaryImplementation = Element.IsRoot || Response.Reference == null;

            // For non-primary implementations (referencing a response in the components section),
            // inherit from the primary implementation
            TypeSyntax baseType = isPrimaryImplementation
                ? ResponsesNamespace.OperationResponse
                : Context.TypeGeneratorRegistry.Get(
                    Context.Document.ResolveComponentReference<OpenApiResponse>(Response.Reference!)).TypeInfo.Name;

            var bodyType = GetSchemaGenerator().schemaGenerator?.TypeInfo.Name;

            var declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(baseType))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    GenerateConstructors(className)
                        .Concat(MethodGenerators.SelectMany(p => p.Generate(Element, className)))
                        .ToArray<MemberDeclarationSyntax>());

            if (Element.IsRoot)
            {
                // Components should be abstract and inherited for each operation
                declaration = declaration.AddModifiers(Token(SyntaxKind.AbstractKeyword));
            }

            if (isPrimaryImplementation)
            {
                declaration = declaration.WithMembers(declaration.Members.AddRange(GenerateHeaderProperties()));

                if (bodyType is not null)
                {
                    declaration = declaration.WithMembers(declaration.Members.Add(GenerateBodyField(bodyType)));

                    // Add the IOperationResponse<TBody> interface for responses with a body
                    declaration = declaration.AddBaseListTypes(SimpleBaseType(ResponsesNamespace.IOperationResponseTBody(bodyType)));
                }
            }

            (ITypeGenerator? schemaGenerator, bool schemaIsReference) = GetSchemaGenerator();
            if (schemaGenerator != null && !schemaIsReference)
            {
                declaration = declaration.WithMembers(declaration.Members.AddRange(schemaGenerator.Generate()));
            }

            yield return declaration;
        }

        private IEnumerable<ConstructorDeclarationSyntax> GenerateConstructors(string className)
        {
            var modifiers = new SyntaxTokenList(Element.IsRoot ? Token(SyntaxKind.ProtectedKeyword) : Token(SyntaxKind.PublicKeyword));

            // Construct from HttpResponseMessage
            yield return ConstructorDeclaration(
                default,
                modifiers,
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

            // Construct from HttpResponseMessage with ITypeSerializerRegistry override
            yield return ConstructorDeclaration(
                default,
                modifiers,
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

                if (!header.IsReference)
                {
                    ILocatedOpenApiElement<OpenApiSchema> schemaElement = header.GetSchemaOrDefault();
                    if (!schemaElement.IsReference)
                    {
                        foreach (var memberDeclaration in headerGenerator.Generate())
                        {
                            yield return memberDeclaration;
                        }
                    }
                }
            }
        }

        [return: NotNullIfNotNull(nameof(bodyType))]
        private MemberDeclarationSyntax? GenerateBodyField(TypeSyntax? bodyType)
        {
            if (bodyType is null)
            {
                return null;
            }

            return FieldDeclaration(
                default,
                new SyntaxTokenList(Token(SyntaxKind.PrivateKeyword)),
                VariableDeclaration(
                    bodyType.MakeNullable(),
                    SingletonSeparatedList(VariableDeclarator(Identifier(BodyFieldName)))));
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

            if (Element.IsRoot)
            {
                // We're in the components section

                return formatter.Format(Response.Reference.Id + "Response");
            }
            else
            {
                // We're in an operation

                ILocatedOpenApiElement<OpenApiOperation> operation = Element.Parents()
                    .OfType<LocatedOpenApiElement<OpenApiOperation>>()
                    .First();

                string? operationName = operationNameProvider.GetOperationName(operation);
                Debug.Assert(operationName is not null);

                string responseCode = Enum.TryParse<HttpStatusCode>(Element.Key, out var statusCode)
                    ? HttpResponseCodeNameProvider.GetName(statusCode)
                    : Element.Key;

                return formatter.Format($"{operationName}{responseCode}Response");
            }
        }
    }
}

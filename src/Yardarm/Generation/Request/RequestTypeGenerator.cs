using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class RequestTypeGenerator : TypeGeneratorBase<OpenApiOperation>
    {
        public const string BodyPropertyName = "Body";

        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected IBuildRequestMethodGenerator BuildRequestMethodGenerator { get; }
        protected IBuildUriMethodGenerator BuildUriMethodGenerator { get; }
        protected IAddHeadersMethodGenerator AddHeadersMethodGenerator { get; }
        protected IBuildContentMethodGenerator BuildContentMethodGenerator { get; }
        protected IRequestsNamespace RequestsNamespace { get; }

        protected OpenApiOperation Operation => Element.Element;

        protected string ClassName => Operation.OperationId + "Request";

        public RequestTypeGenerator(ILocatedOpenApiElement<OpenApiOperation> operationElement,
            GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IBuildRequestMethodGenerator buildRequestMethodGenerator, IBuildUriMethodGenerator buildUriMethodGenerator,
            IAddHeadersMethodGenerator addHeadersMethodGenerator, IBuildContentMethodGenerator buildContentMethodGenerator,
            IRequestsNamespace requestsNamespace)
            : base(operationElement, context, null)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            BuildRequestMethodGenerator = buildRequestMethodGenerator ?? throw new ArgumentNullException(nameof(buildRequestMethodGenerator));
            BuildUriMethodGenerator = buildUriMethodGenerator ??
                                      throw new ArgumentNullException(nameof(buildUriMethodGenerator));
            AddHeadersMethodGenerator = addHeadersMethodGenerator ??
                                        throw new ArgumentNullException(nameof(addHeadersMethodGenerator));
            BuildContentMethodGenerator = buildContentMethodGenerator ??
                                          throw new ArgumentNullException(nameof(buildContentMethodGenerator));
            RequestsNamespace = requestsNamespace ?? throw new ArgumentNullException(nameof(requestsNamespace));
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Interface);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return new YardarmTypeInfo(QualifiedName(ns,
                IdentifierName(formatter.Format(ClassName))));
        }

        public override QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child,
            NameKind nameKind) =>
            QualifiedName((NameSyntax)TypeInfo.Name, IdentifierName(
                Context.NameFormatterSelector.GetFormatter(nameKind).Format(child.Key + "-Model")));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            yield return GenerateInterface();

            yield return GenerateClass();
        }

        protected virtual InterfaceDeclarationSyntax GenerateInterface()
        {
            string interfaceName = ((QualifiedNameSyntax)TypeInfo.Name).Right.Identifier.ValueText;

            InterfaceDeclarationSyntax declaration = InterfaceDeclaration(interfaceName)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(RequestsNamespace.IOperationRequest));

            declaration = declaration.AddMembers(
                GenerateParameterProperties(Element.GetParameters(), false).ToArray());

            return declaration.AddMembers(
                BuildRequestMethodGenerator.GenerateHeader(Element));
        }

        protected virtual ClassDeclarationSyntax GenerateClass()
        {
            string className = Context.NameFormatterSelector.GetFormatter(NameKind.Class).Format(ClassName);

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(
                    SimpleBaseType(RequestsNamespace.OperationRequest),
                    SimpleBaseType(TypeInfo.Name))
                .AddMembers(ConstructorDeclaration(className)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithBody(Block()));

            declaration = declaration.AddMembers(
                GenerateParameterProperties(Element.GetParameters(), true).ToArray());

            var requestBodyElement = Element.GetRequestBody();
            if (requestBodyElement != null)
            {
                var schema = MediaTypeSelector.Select(requestBodyElement)?.GetSchema();
                if (schema != null)
                {
                    declaration = declaration.AddMembers(
                        CreatePropertyDeclaration(requestBodyElement, className, schema, "Body")
                            .AddModifiers(Token(SyntaxKind.PublicKeyword)));
                }
            }

            return declaration.AddMembers(
                BuildRequestMethodGenerator.Generate(Element),
                BuildUriMethodGenerator.Generate(Element),
                AddHeadersMethodGenerator.Generate(Element),
                BuildContentMethodGenerator.Generate(Element));
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateParameterProperties(
            IEnumerable<ILocatedOpenApiElement<OpenApiParameter>> properties, bool forClass)
        {
            foreach (var parameter in properties)
            {
                var schema = parameter.GetSchemaOrDefault();

                PropertyDeclarationSyntax propertyDeclaration = CreatePropertyDeclaration(parameter, ClassName, schema);
                if (forClass)
                {
                    propertyDeclaration = propertyDeclaration.AddModifiers(Token(SyntaxKind.PublicKeyword));
                }

                yield return propertyDeclaration;

                if (!forClass && parameter.Element.Reference == null && schema.Element.Reference == null)
                {
                    foreach (var member in Context.TypeGeneratorRegistry.Get(schema).Generate())
                    {
                        yield return member;
                    }
                }
            }
        }

        protected virtual PropertyDeclarationSyntax CreatePropertyDeclaration<T>(ILocatedOpenApiElement<T> parameter, string className,
            ILocatedOpenApiElement<OpenApiSchema> schema, string? nameOverride = null)
            where T : IOpenApiElement
        {
            string propertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property).Format(
                nameOverride ?? parameter.Key);

            if (propertyName == className)
            {
                // Properties can't have the same name as the class/interface
                propertyName += "Value";
            }

            var typeName = Context.TypeGeneratorRegistry.Get(schema).TypeInfo.Name;

            var propertyDeclaration = PropertyDeclaration(typeName, propertyName)
                .AddElementAnnotation(parameter, Context.ElementRegistry)
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

            return propertyDeclaration;
        }
    }
}

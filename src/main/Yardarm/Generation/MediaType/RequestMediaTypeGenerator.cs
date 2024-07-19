using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Generation.Request;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.MediaType
{
    public class RequestMediaTypeGenerator(
        ILocatedOpenApiElement<OpenApiMediaType> mediaTypeElement,
        ITypeGenerator parent,
        GenerationContext context,
        IRequestsNamespace requestsNamespace,
        ISerializerSelector serializerSelector,
        IEnumerable<IRequestMemberGenerator> memberGenerators,
        IOperationNameProvider operationNameProvider)
        : TypeGeneratorBase<OpenApiMediaType>(mediaTypeElement, context, parent)
    {
        public const string BodyPropertyName = "Body";

        protected OpenApiMediaType MediaType => Element.Element;

        protected RequestTypeGenerator RequestTypeGenerator { get; } =
            FindParentRequestTypeGenerator(parent)
                ?? throw new InvalidOperationException($"Must be the child of a {nameof(RequestTypeGenerator)}");

        protected IRequestsNamespace RequestsNamespace { get; } = requestsNamespace;
        protected ISerializerSelector SerializerSelector { get; } = serializerSelector;
        protected IEnumerable<IRequestMemberGenerator> MemberGenerators { get; } = memberGenerators;

        private static RequestTypeGenerator? FindParentRequestTypeGenerator(ITypeGenerator? generator)
        {
            while (generator != null)
            {
                if (generator is RequestTypeGenerator requestTypeGenerator)
                {
                    return requestTypeGenerator;
                }

                generator = generator.Parent;
            }

            return null;
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            SerializerDescriptor? serializerDescriptor = SerializerSelector.Select(Element)?.Descriptor;
            if (serializerDescriptor == null)
            {
                throw new InvalidOperationException($"No serializer configured for {Element}.");
            }

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(RequestTypeGenerator.Element);

            string? operationName = operationNameProvider.GetOperationName(RequestTypeGenerator.Element);
            Debug.Assert(operationName is not null);

            TypeSyntax name = QualifiedName(ns,
                IdentifierName(formatter.Format($"{operationName}-{serializerDescriptor.NameSegment}-Request")));

            return new YardarmTypeInfo(name);
        }

        public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child,
            NameKind nameKind) =>
            QualifiedName((NameSyntax)TypeInfo.Name, IdentifierName(
                Context.NameFormatterSelector.GetFormatter(nameKind).Format(child.Key + "-Body")));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = ((QualifiedNameSyntax)TypeInfo.Name).Right.Identifier.ValueText;

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddGeneratorAnnotation(this)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(RequestTypeGenerator.TypeInfo.Name))
                .AddMembers(ConstructorDeclaration(className)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithBody(Block()));

            var schema = Element.GetSchemaOrDefault();
            declaration = declaration.AddMembers(CreateBodyPropertyDeclaration(schema));

            if (schema.Element.Reference == null)
            {
                ITypeGenerator schemaGenerator = Context.TypeGeneratorRegistry.Get(schema);

                MemberDeclarationSyntax[] childMembers = schemaGenerator.Generate().ToArray();
                if (childMembers.Length > 0)
                {
                    declaration = declaration.AddMembers(childMembers);
                }
            }

            yield return declaration.AddMembers(
                MemberGenerators.SelectMany(p => p.Generate(RequestTypeGenerator.Element, Element))
                    .ToArray());
        }

        protected virtual PropertyDeclarationSyntax CreateBodyPropertyDeclaration(ILocatedOpenApiElement<OpenApiSchema> schema)
        {
            var typeName = Context.TypeGeneratorRegistry.Get(schema).TypeInfo.Name;

            var propertyDeclaration = PropertyDeclaration(typeName, BodyPropertyName)
                .AddElementAnnotation(schema, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

            return propertyDeclaration;
        }
    }
}

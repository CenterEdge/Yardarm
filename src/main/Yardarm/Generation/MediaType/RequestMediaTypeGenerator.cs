using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Request;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.MediaType
{
    public class RequestMediaTypeGenerator : TypeGeneratorBase<OpenApiMediaType>
    {
        public const string BodyPropertyName = "Body";

        protected OpenApiMediaType MediaType => Element.Element;

        protected RequestTypeGenerator RequestTypeGenerator { get; }

        protected IRequestsNamespace RequestsNamespace { get; }
        protected ISerializerSelector SerializerSelector { get; }
        protected IList<IRequestMemberGenerator> MemberGenerators { get; }

        public RequestMediaTypeGenerator(ILocatedOpenApiElement<OpenApiMediaType> mediaTypeElement,
            GenerationContext context, ITypeGenerator parent, IRequestsNamespace requestsNamespace,
            ISerializerSelector serializerSelector, IList<IRequestMemberGenerator> memberGenerators)
            : base(mediaTypeElement, context, parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            RequestsNamespace = requestsNamespace ?? throw new ArgumentNullException(nameof(requestsNamespace));
            SerializerSelector = serializerSelector ?? throw new ArgumentNullException(nameof(serializerSelector));
            MemberGenerators = memberGenerators ??
                               throw new ArgumentNullException(nameof(memberGenerators));

            RequestTypeGenerator = FindParentRequestTypeGenerator(parent)
                                   ?? throw new InvalidOperationException(
                                       $"Must be the child of a {nameof(RequestTypeGenerator)}");
        }

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

            TypeSyntax name = QualifiedName(ns,
                IdentifierName(formatter.Format($"{RequestTypeGenerator.Element.Element.OperationId}-{serializerDescriptor.NameSegment}-Request")));

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

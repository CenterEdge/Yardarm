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

        protected OpenApiOperation Operation => Element.Element;

        public RequestTypeGenerator(LocatedOpenApiElement<OpenApiOperation> operationElement,
            GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IBuildRequestMethodGenerator buildRequestMethodGenerator, IBuildUriMethodGenerator buildUriMethodGenerator,
            IAddHeadersMethodGenerator addHeadersMethodGenerator, IBuildContentMethodGenerator buildContentMethodGenerator)
            : base(operationElement, context)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            BuildRequestMethodGenerator = buildRequestMethodGenerator ?? throw new ArgumentNullException(nameof(buildRequestMethodGenerator));
            BuildUriMethodGenerator = buildUriMethodGenerator ??
                                      throw new ArgumentNullException(nameof(buildUriMethodGenerator));
            AddHeadersMethodGenerator = addHeadersMethodGenerator ??
                                        throw new ArgumentNullException(nameof(addHeadersMethodGenerator));
            BuildContentMethodGenerator = buildContentMethodGenerator ??
                                          throw new ArgumentNullException(nameof(buildContentMethodGenerator));
        }

        protected override TypeSyntax GetTypeName()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return QualifiedName(ns,
                IdentifierName(formatter.Format(Operation.OperationId + "Request")));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            string className = classNameAndNamespace.Right.Identifier.Text;

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(ConstructorDeclaration(className)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithBody(Block()));

            declaration = AddParameterProperties(declaration,
                Operation.Parameters.Select(p => Element.CreateChild(p, p.Name)));

            if (Operation.RequestBody != null)
            {
                var requestBodyElement = Element.CreateChild(Operation.RequestBody, BodyPropertyName);
                var schema = MediaTypeSelector.Select(requestBodyElement)?.Element.Schema;
                if (schema != null)
                {
                    var locatedSchema = requestBodyElement.CreateChild(schema, "");

                    declaration = declaration.AddMembers(
                        CreatePropertyDeclaration(requestBodyElement, className, locatedSchema));
                }
            }

            declaration = declaration.AddMembers(
                BuildRequestMethodGenerator.Generate(Element),
                BuildUriMethodGenerator.Generate(Element),
                AddHeadersMethodGenerator.Generate(Element),
                BuildContentMethodGenerator.Generate(Element));

            yield return declaration;
        }

        protected virtual ClassDeclarationSyntax AddParameterProperties(ClassDeclarationSyntax declaration,
            IEnumerable<LocatedOpenApiElement<OpenApiParameter>> properties)
        {
            MemberDeclarationSyntax[] members = properties
                .Select(p =>
                {
                    var schema = p.CreateChild(p.Element.Schema, "");

                    return CreatePropertyDeclaration(p, declaration.Identifier.ValueText, schema);
                })
                .ToArray<MemberDeclarationSyntax>();

            return declaration.AddMembers(members);
        }

        protected virtual PropertyDeclarationSyntax CreatePropertyDeclaration<T>(LocatedOpenApiElement<T> parameter, string className,
            LocatedOpenApiElement<OpenApiSchema> schema)
            where T : IOpenApiElement
        {
            string propertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property).Format(parameter.Key);

            if (propertyName == className)
            {
                // Properties can't have the same name as the class/interface
                propertyName += "Value";
            }

            var typeName = Context.TypeNameProvider.GetName(schema);

            var propertyDeclaration = PropertyDeclaration(typeName, propertyName)
                .AddElementAnnotation(parameter, Context.ElementRegistry)
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

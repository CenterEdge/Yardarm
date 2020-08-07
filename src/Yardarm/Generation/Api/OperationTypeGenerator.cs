using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;

namespace Yardarm.Generation.Api
{
    public class OperationTypeGenerator : TypeGeneratorBase<OpenApiOperation>
    {
        protected IMediaTypeSelector MediaTypeSelector { get; }

        protected OpenApiOperation Operation => Element.Element;

        public OperationTypeGenerator(LocatedOpenApiElement<OpenApiOperation> operationElement,
            GenerationContext context, IMediaTypeSelector mediaTypeSelector)
            : base(operationElement, context)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public override TypeSyntax GetTypeName()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return SyntaxFactory.QualifiedName(ns,
                SyntaxFactory.IdentifierName(formatter.Format(Operation.OperationId + "Request")));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            string className = classNameAndNamespace.Right.Identifier.Text;

            ClassDeclarationSyntax declaration = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block()));

            declaration = AddProperties(declaration, Element, Operation.Parameters);

            if (Operation.RequestBody != null)
            {
                var requestBodyElement = Element.CreateChild(Operation.RequestBody, "Body");
                if (MediaTypeSelector.Select(requestBodyElement)?.Element.Schema != null)
                {
                    declaration = declaration.AddMembers(
                        CreatePropertyDeclaration(requestBodyElement, className));
                }
            }

            yield return declaration;
        }

        protected virtual ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax declaration,
            LocatedOpenApiElement<OpenApiOperation> parent, IEnumerable<OpenApiParameter> properties)
        {
            MemberDeclarationSyntax[] members = properties
                .Select(p => CreatePropertyDeclaration(parent.CreateChild(p.Schema, p.Name), declaration.Identifier.ValueText))
                .ToArray();

            return declaration.AddMembers(members);
        }

        protected virtual MemberDeclarationSyntax CreatePropertyDeclaration(LocatedOpenApiElement property, string ownerName)
        {
            string propertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property).Format(property.Key);

            if (propertyName == ownerName)
            {
                // Properties can't have the same name as the class/interface
                propertyName += "Value";
            }

            var typeName = Context.TypeNameGenerator.GetName(property);

            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(typeName, propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            return propertyDeclaration;
        }
    }
}

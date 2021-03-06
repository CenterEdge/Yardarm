﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request.Internal
{
    /// <summary>
    /// Delivers an inherited request class which handles raw HttpContent
    /// </summary>
    internal class HttpContentRequestTypeGenerator : TypeGeneratorBase<OpenApiOperation>
    {
        private readonly IBuildContentMethodGenerator _buildContentMethodGenerator;

        private RequestTypeGenerator RequestTypeGenerator => (RequestTypeGenerator)Parent!;

        public HttpContentRequestTypeGenerator(ILocatedOpenApiElement<OpenApiOperation> element, GenerationContext context,
            RequestTypeGenerator parent, IBuildContentMethodGenerator buildContentMethodGenerator)
            : base(element, context, parent)
        {
            _buildContentMethodGenerator = buildContentMethodGenerator ?? throw new ArgumentNullException(nameof(buildContentMethodGenerator));
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(RequestTypeGenerator.Element);

            TypeSyntax name = QualifiedName(ns,
                IdentifierName(formatter.Format($"{RequestTypeGenerator.Element.Element.OperationId}-HttpContent-Request")));

            return new YardarmTypeInfo(name);
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = ((QualifiedNameSyntax)TypeInfo.Name).Right.Identifier.ValueText;

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(RequestTypeGenerator.TypeInfo.Name))
                .AddMembers(ConstructorDeclaration(className)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .WithBody(Block()));

            declaration = declaration.AddMembers(new MemberDeclarationSyntax[]
            {
                CreateBodyPropertyDeclaration(),
                _buildContentMethodGenerator.GenerateHeader(RequestTypeGenerator.Element)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(
                        IdentifierName(RequestMediaTypeGenerator.BodyPropertyName)))
            });

            yield return declaration;
        }

        protected virtual PropertyDeclarationSyntax CreateBodyPropertyDeclaration()
        {
            TypeSyntax typeName = NullableType(WellKnownTypes.System.Net.Http.HttpContent.Name);

            var propertyDeclaration = PropertyDeclaration(typeName, RequestMediaTypeGenerator.BodyPropertyName)
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

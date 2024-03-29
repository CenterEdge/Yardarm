﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Authentication
{
    public abstract class SecuritySchemeTypeGenerator : TypeGeneratorBase<OpenApiSecurityScheme>
    {
        private static readonly SyntaxTokenList _withAsyncModifiers = new SyntaxTokenList(
            Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword), Token(SyntaxKind.AsyncKeyword));

        private static readonly SyntaxTokenList _withoutAsyncModifiers = new SyntaxTokenList(
            Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword));

        protected const string MessageParameterName = "message";

        protected IAuthenticationNamespace AuthenticationNamespace { get; set; }

        protected OpenApiSecurityScheme SecurityScheme => Element.Element;

        protected virtual bool ApplyUsesAsyncAwait => false;

        protected virtual bool ProcessResponseUsesAsyncAwait => false;

        protected SecuritySchemeTypeGenerator(ILocatedOpenApiElement<OpenApiSecurityScheme> securitySchemeElement, GenerationContext context,
            IAuthenticationNamespace authenticationNamespace)
            : base(securitySchemeElement, context, null)
        {
            ArgumentNullException.ThrowIfNull(authenticationNamespace);

            AuthenticationNamespace = authenticationNamespace;
        }

        protected override YardarmTypeInfo GetTypeInfo() =>
            new YardarmTypeInfo(QualifiedName(
                Context.NamespaceProvider.GetNamespace(Element),
                IdentifierName(GetClassName())));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = GetClassName();

            var declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(AuthenticationNamespace.IAuthenticator))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(GenerateAdditionalMembers(className)
                    .Concat(new MemberDeclarationSyntax[]
                    {
                        GenerateApplyAsyncMethod(),
                        GenerateProcessResponseAsyncMethod()
                    })
                    .ToArray());

            yield return declaration;
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateAdditionalMembers(string className) =>
            Enumerable.Empty<MemberDeclarationSyntax>();

        protected virtual MethodDeclarationSyntax GenerateApplyAsyncMethod() =>
            MethodDeclaration(WellKnownTypes.System.Threading.Tasks.ValueTask.Name, "ApplyAsync")
                .WithModifiers(ApplyUsesAsyncAwait ? _withAsyncModifiers : _withoutAsyncModifiers)
                .WithLeadingTrivia(DocumentationSyntaxHelpers.InheritDocTrivia)
                .AddParameterListParameters(
                    Parameter(Identifier(MessageParameterName))
                        .WithType(WellKnownTypes.System.Net.Http.HttpRequestMessage.Name),
                    MethodHelpers.DefaultedCancellationTokenParameter())
                .WithBody(GenerateApplyAsyncBody());

        protected virtual MethodDeclarationSyntax GenerateProcessResponseAsyncMethod() =>
            MethodDeclaration(WellKnownTypes.System.Threading.Tasks.ValueTask.Name, "ProcessResponseAsync")
                .WithModifiers(ProcessResponseUsesAsyncAwait ? _withAsyncModifiers : _withoutAsyncModifiers)
                .WithLeadingTrivia(DocumentationSyntaxHelpers.InheritDocTrivia)
                .AddParameterListParameters(
                    Parameter(Identifier(MessageParameterName))
                        .WithType(WellKnownTypes.System.Net.Http.HttpResponseMessage.Name),
                    MethodHelpers.DefaultedCancellationTokenParameter())
                .WithBody(GenerateProcessResponseAsyncBody());

        protected abstract BlockSyntax GenerateApplyAsyncBody();

        protected virtual BlockSyntax GenerateProcessResponseAsyncBody() => Block(
            MethodHelpers.ThrowIfArgumentNull(MessageParameterName),

            ReturnStatement(LiteralExpression(SyntaxKind.DefaultLiteralExpression)));

        protected virtual string GetClassName() => Context.NameFormatterSelector.GetFormatter(NameKind.Class)
            .Format(Element.Element.Reference.Id);
    }
}

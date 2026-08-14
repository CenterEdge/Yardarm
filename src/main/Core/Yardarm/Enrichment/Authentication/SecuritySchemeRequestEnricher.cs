using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Generation;
using Yardarm.Generation.Request;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Authentication
{
    public class SecuritySchemeRequestEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiOperation>
    {
        private static readonly SyntaxTriviaList s_disableObsoleteWarningTrivia =
            ParseLeadingTrivia("#pragma warning disable CS0618\n");

        private static readonly SyntaxTriviaList s_restoreObsoleteWarningTrivia =
            ParseTrailingTrivia("\n#pragma warning restore CS0618\n");

        private readonly GenerationContext _context;
        private readonly IAuthenticationNamespace _authenticationNamespace;

        public SecuritySchemeRequestEnricher(GenerationContext context, IAuthenticationNamespace authenticationNamespace)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(authenticationNamespace);

            _context = context;
            _authenticationNamespace = authenticationNamespace;
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiOperation> context) =>
            context.Element.Security is { Count: > 0 } && target.GetGeneratorAnnotation() == typeof(RequestTypeGenerator)
                ? AddSecuritySchemes(target, context.LocatedElement)
                : target;

        private ClassDeclarationSyntax AddSecuritySchemes(ClassDeclarationSyntax target, ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            var className = IdentifierName(target.Identifier);

            var attributes = new List<AttributeListSyntax>();

            foreach (var securityRequirement in operation.GetSecurityRequirements())
            {
                ILocatedOpenApiElement<IOpenApiSecurityScheme>[] securitySchemes = securityRequirement.GetSecuritySchemes()
                    .Select(p => p.Key)
                    .ToArray();

                attributes.Add(SuppressObsoleteWarning(
                    AttributeList(SingletonSeparatedList(
                        Attribute(_authenticationNamespace.SecuritySchemeSetAttribute)
                            .AddArgumentListArguments(
                                securitySchemes.Select(securityScheme =>
                                        AttributeArgument(TypeOfExpression(_context.TypeGeneratorRegistry.Get(securityScheme).TypeInfo.Name)))
                                    .ToArray())))));

                if (securitySchemes.Length == 1)
                {
                    TypeSyntax schemeTypeName = _context.TypeGeneratorRegistry.Get(securitySchemes[0]).TypeInfo.Name;

                    target = target.AddMembers(SuppressObsoleteWarning(MethodDeclaration(className, "WithAuthenticator")
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            Parameter(Identifier("authenticator"))
                                .WithType(schemeTypeName))
                        .WithBody(Block(
                            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("Authenticator"),
                                IdentifierName("authenticator"))),
                            ReturnStatement(ThisExpression())))));
                }
                else if (securitySchemes.Length > 1)
                {
                    target = target.AddMembers(SuppressObsoleteWarning(MethodDeclaration(className, "WithAuthenticator")
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            securitySchemes
                                .Select((p, index) =>
                                    Parameter(Identifier($"authenticator{index}"))
                                        .WithType(_context.TypeGeneratorRegistry.Get(p).TypeInfo.Name))
                                .ToArray())
                        .WithBody(Block(
                            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("Authenticator"),
                                ObjectCreationExpression(_authenticationNamespace.MultiAuthenticator)
                                    .AddArgumentListArguments(
                                        securitySchemes
                                            .Select((_, index) => Argument(IdentifierName($"authenticator{index}")))
                                            .ToArray()))),
                            ReturnStatement(ThisExpression())))));
                }
            }

            if (attributes.Count > 0)
            {
                target = target.AddAttributeLists(attributes.ToArray());
            }

            return target;
        }

        private static AttributeListSyntax SuppressObsoleteWarning(AttributeListSyntax attributeList) =>
            attributeList
                .WithLeadingTrivia(s_disableObsoleteWarningTrivia)
                .WithTrailingTrivia(s_restoreObsoleteWarningTrivia);

        private static MethodDeclarationSyntax SuppressObsoleteWarning(MethodDeclarationSyntax method) =>
            method
                .WithLeadingTrivia(s_disableObsoleteWarningTrivia)
                .WithTrailingTrivia(s_restoreObsoleteWarningTrivia);
    }
}

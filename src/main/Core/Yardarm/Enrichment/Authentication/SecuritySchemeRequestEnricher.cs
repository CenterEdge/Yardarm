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
            ParseLeadingTrivia("#pragma warning restore CS0618\n");

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

            var attributes = new List<(AttributeListSyntax Attribute, bool IsDeprecated)>();

            foreach (var securityRequirement in operation.GetSecurityRequirements())
            {
                ILocatedOpenApiElement<IOpenApiSecurityScheme>[] securitySchemes = securityRequirement.GetSecuritySchemes()
                    .Select(p => p.Key)
                    .ToArray();

                bool isDeprecated = securitySchemes.Any(p => p.Element.Deprecated);
                var attribute = AttributeList(SingletonSeparatedList(
                        Attribute(_authenticationNamespace.SecuritySchemeSetAttribute)
                            .AddArgumentListArguments(
                                securitySchemes.Select(securityScheme =>
                                        AttributeArgument(TypeOfExpression(_context.TypeGeneratorRegistry.Get(securityScheme).TypeInfo.Name)))
                                    .ToArray())))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed);
                attributes.Add((attribute, isDeprecated));

                if (securitySchemes.Length == 1)
                {
                    TypeSyntax schemeTypeName = _context.TypeGeneratorRegistry.Get(securitySchemes[0]).TypeInfo.Name;

                    var method = MethodDeclaration(className, "WithAuthenticator")
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            Parameter(Identifier("authenticator"))
                                .WithType(schemeTypeName))
                        .WithBody(Block(
                            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("Authenticator"),
                                IdentifierName("authenticator"))),
                            ReturnStatement(ThisExpression())));

                    target = target.AddMembers(isDeprecated ? SuppressObsoleteWarning(method) : method);
                }
                else if (securitySchemes.Length > 1)
                {
                    var method = MethodDeclaration(className, "WithAuthenticator")
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
                            ReturnStatement(ThisExpression())));

                    target = target.AddMembers(isDeprecated ? SuppressObsoleteWarning(method) : method);
                }
            }

            if (attributes.Count > 0)
            {
                bool restoreWarning = false;
                var attributeLists = new List<AttributeListSyntax>(attributes.Count);
                foreach ((AttributeListSyntax attribute, bool isDeprecated) in attributes)
                {
                    attributeLists.Add(ApplyObsoleteWarningDirectives(attribute, restoreWarning, isDeprecated));
                    restoreWarning = isDeprecated;
                }

                target = target.AddAttributeLists(attributeLists.ToArray());
                if (restoreWarning)
                {
                    target = RestoreObsoleteWarning(target);
                }
            }

            return target;
        }

        private static AttributeListSyntax ApplyObsoleteWarningDirectives(AttributeListSyntax attributeList,
            bool restoreWarning, bool suppressWarning)
        {
            SyntaxTriviaList leadingTrivia = attributeList.GetLeadingTrivia();
            if (suppressWarning)
            {
                leadingTrivia = s_disableObsoleteWarningTrivia.AddRange(leadingTrivia);
            }

            if (restoreWarning)
            {
                leadingTrivia = s_restoreObsoleteWarningTrivia.AddRange(leadingTrivia);
            }

            return attributeList.WithLeadingTrivia(leadingTrivia);
        }

        private static MethodDeclarationSyntax SuppressObsoleteWarning(MethodDeclarationSyntax method) =>
            method
                .WithLeadingTrivia(s_disableObsoleteWarningTrivia)
                .WithBody(method.Body!.WithOpenBraceToken(method.Body.OpenBraceToken.WithLeadingTrivia(
                    s_restoreObsoleteWarningTrivia.AddRange(method.Body.OpenBraceToken.LeadingTrivia))));

        private static ClassDeclarationSyntax RestoreObsoleteWarning(ClassDeclarationSyntax declaration)
        {
            SyntaxToken firstModifier = declaration.Modifiers.First();
            return declaration.WithModifiers(declaration.Modifiers.Replace(firstModifier,
                firstModifier.WithLeadingTrivia(s_restoreObsoleteWarningTrivia.AddRange(firstModifier.LeadingTrivia))));
        }
    }
}

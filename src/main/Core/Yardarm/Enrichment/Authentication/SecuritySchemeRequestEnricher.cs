using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Generation;
using Yardarm.Generation.Request;
using Yardarm.Helpers;
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
                AttributeListSyntax attribute = AttributeList(SingletonSeparatedList(
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

                    MethodDeclarationSyntax method = MethodDeclaration(className, "WithAuthenticator")
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            Parameter(Identifier("authenticator"))
                                .WithType(schemeTypeName))
                        .WithBody(Block(
                            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("Authenticator"),
                                IdentifierName("authenticator"))),
                            ReturnStatement(ThisExpression())));

                    target = target.AddMembers(isDeprecated
                        ? MarkObsolete(method)
                        : method);
                }
                else if (securitySchemes.Length > 1)
                {
                    MethodDeclarationSyntax method = MethodDeclaration(className, "WithAuthenticator")
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

                    target = target.AddMembers(isDeprecated ?
                        MarkObsolete(method)
                        : method);
                }
            }

            if (attributes.Count > 0)
            {
                bool isSuppressed = false;
                var attributeLists = new List<AttributeListSyntax>(attributes.Count);

                foreach ((AttributeListSyntax attribute, bool isDeprecated) in attributes)
                {
                    if (isDeprecated)
                    {
                        attributeLists.Add(!isSuppressed
                            ? ApplyObsoleteWarningDirectives(attribute, restoreWarning: false)
                            : attribute);

                        isSuppressed = true;
                    }
                    else
                    {
                        attributeLists.Add(isSuppressed
                            ? ApplyObsoleteWarningDirectives(attribute, restoreWarning: true)
                            : attribute);
                    }
                }

                target = target.AddAttributeLists([..attributeLists]);

                // Final warning restore
                if (isSuppressed)
                {
                    target = RestoreObsoleteWarning(target);
                }
            }

            return target;
        }

        private static AttributeListSyntax ApplyObsoleteWarningDirectives(AttributeListSyntax attributeList,
            bool restoreWarning)
        {
            SyntaxTriviaList leadingTrivia = attributeList.GetLeadingTrivia();
            if (restoreWarning)
            {
                leadingTrivia = s_restoreObsoleteWarningTrivia.AddRange(leadingTrivia);
            }
            else
            {
                leadingTrivia = s_disableObsoleteWarningTrivia.AddRange(leadingTrivia);
            }

            return attributeList.WithLeadingTrivia(leadingTrivia);
        }

        private static ClassDeclarationSyntax RestoreObsoleteWarning(ClassDeclarationSyntax declaration)
        {
            SyntaxToken firstModifier = declaration.Modifiers.First();
            return declaration.WithModifiers(declaration.Modifiers.Replace(firstModifier,
                firstModifier.WithLeadingTrivia(s_restoreObsoleteWarningTrivia.AddRange(firstModifier.LeadingTrivia))));
        }

        private static MethodDeclarationSyntax MarkObsolete(MethodDeclarationSyntax method)
        {
            AttributeArgumentSyntax argument = AttributeArgument(SyntaxHelpers.StringLiteral("Security scheme has been deprecated."));
            AttributeListSyntax attribute = AttributeList(SingletonSeparatedList(
                    Attribute(WellKnownTypes.System.ObsoleteAttribute.Name,
                        AttributeArgumentList(SingletonSeparatedList(argument)))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed);

            return method.AddAttributeLists(attribute);
        }
    }
}

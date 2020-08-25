using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Authentication.Internal
{
    internal class SecuritySchemeRequestEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiOperation>
    {
        private readonly GenerationContext _context;
        private readonly IAuthenticationNamespace _authenticationNamespace;

        public int Priority => 0;

        public SecuritySchemeRequestEnricher(GenerationContext context, IAuthenticationNamespace authenticationNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _authenticationNamespace = authenticationNamespace ?? throw new ArgumentNullException(nameof(authenticationNamespace));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiOperation> context) =>
            context.Element.Security.Count > 0
                ? AddSecuritySchemes(target, context.LocatedElement)
                : target;

        private ClassDeclarationSyntax AddSecuritySchemes(ClassDeclarationSyntax target, ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            var className = IdentifierName(target.Identifier);

            var attributes = new List<AttributeSyntax>();

            foreach (var securityRequirement in operation.GetSecurityRequirements())
            {
                ILocatedOpenApiElement<OpenApiSecurityScheme>[] securitySchemes = securityRequirement.GetSecuritySchemes()
                    .Select(p => p.Key)
                    .ToArray();

                attributes.Add(Attribute(_authenticationNamespace.SecuritySchemeSetAttribute)
                    .AddArgumentListArguments(
                        securitySchemes.Select(securityScheme =>
                                AttributeArgument(TypeOfExpression(_context.TypeNameProvider.GetName(securityScheme))))
                            .ToArray()));

                if (securitySchemes.Length == 1)
                {
                    TypeSyntax schemeTypeName = _context.TypeNameProvider.GetName(securitySchemes[0]);

                    target = target.AddMembers(MethodDeclaration(className, "WithAuthenticator")
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            Parameter(Identifier("authenticator"))
                                .WithType(schemeTypeName))
                        .WithBody(Block(
                            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("Authenticator"),
                                IdentifierName("authenticator"))),
                            ReturnStatement(ThisExpression()))));
                }
                else if (securitySchemes.Length > 1)
                {
                    target = target.AddMembers(MethodDeclaration(className, "WithAuthenticator")
                        .AddModifiers(Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            securitySchemes
                                .Select((p, index) =>
                                    Parameter(Identifier($"authenticator{index}"))
                                        .WithType(_context.TypeNameProvider.GetName(p)))
                                .ToArray())
                        .WithBody(Block(
                            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("Authenticator"),
                                ObjectCreationExpression(_authenticationNamespace.MultiAuthenticator)
                                    .AddArgumentListArguments(
                                        securitySchemes
                                            .Select((_, index) => Argument(IdentifierName($"authenticator{index}")))
                                            .ToArray()))),
                            ReturnStatement(ThisExpression()))));
                }
            }

            if (attributes.Count > 0)
            {
                target = target.AddAttributeLists(AttributeList(null, SeparatedList(attributes)));
            }

            return target;
        }
    }
}

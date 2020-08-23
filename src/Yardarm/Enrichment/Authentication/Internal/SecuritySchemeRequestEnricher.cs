using System;
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

            foreach (var securityRequirement in operation.GetSecurityRequirements())
            {
                // TODO: Support logical AND schemes
                var securityScheme = securityRequirement.GetSecuritySchemes().First();

                var schemeTypeName = _context.TypeNameProvider.GetName(securityScheme.Key);

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

            return target;
        }
    }
}

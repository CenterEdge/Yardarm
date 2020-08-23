using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Authentication
{
    public class BearerSecuritySchemeTypeGenerator : SecuritySchemeTypeGenerator
    {
        public const string BearerTokenPropertyName = "BearerToken";

        public BearerSecuritySchemeTypeGenerator(ILocatedOpenApiElement<OpenApiSecurityScheme> securitySchemeElement, GenerationContext context,
            IAuthenticationNamespace authenticationNamespace)
            : base(securitySchemeElement, context, authenticationNamespace)
        {
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateAdditionalMembers(string className)
        {
            yield return ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithBody(Block());

            yield return PropertyDeclaration(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                    BearerTokenPropertyName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }

        protected override BlockSyntax GenerateApplyAsyncBody() =>
            Block(
                MethodHelpers.ThrowIfArgumentNull(MessageParameterName),
                MethodHelpers.IfNotNull(IdentifierName(BearerTokenPropertyName), Block(
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(MessageParameterName),
                                IdentifierName("Headers")),
                            IdentifierName("Authorization")),
                        ObjectCreationExpression(WellKnownTypes.System.Net.Http.Headers.AuthenticationHeaderValue.Name)
                            .AddArgumentListArguments(
                                Argument(SyntaxHelpers.StringLiteral("Bearer")),
                                Argument(IdentifierName(BearerTokenPropertyName))))))));
    }
}

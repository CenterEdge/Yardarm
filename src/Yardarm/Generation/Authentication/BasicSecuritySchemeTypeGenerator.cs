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
    public class BasicSecuritySchemeTypeGenerator : SecuritySchemeTypeGenerator
    {
        public const string UsernameFieldName = "_username";
        public const string UsernamePropertyName = "Username";
        public const string PasswordFieldName = "_password";
        public const string PasswordPropertyName = "Password";
        public const string CacheFieldName = "_cache";

        public BasicSecuritySchemeTypeGenerator(ILocatedOpenApiElement<OpenApiSecurityScheme> securitySchemeElement, GenerationContext context,
            IAuthenticationNamespace authenticationNamespace)
            : base(securitySchemeElement, context, authenticationNamespace)
        {
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateAdditionalMembers(string className)
        {
            yield return ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithBody(Block());

            yield return FieldDeclaration(VariableDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)))
                    .AddVariables(
                        VariableDeclarator(Identifier(UsernameFieldName), null,
                            EqualsValueClause(SyntaxHelpers.StringLiteral(""))),
                        VariableDeclarator(Identifier(PasswordFieldName), null,
                            EqualsValueClause(SyntaxHelpers.StringLiteral("")))))
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            yield return FieldDeclaration(VariableDeclaration(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))))
                    .AddVariables(
                        VariableDeclarator(Identifier(CacheFieldName))))
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            yield return PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)),
                    UsernamePropertyName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(ArrowExpressionClause(IdentifierName(UsernameFieldName))),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, Block(
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(UsernameFieldName),
                            MethodHelpers.ArgumentOrThrowIfNull("value"))),
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(CacheFieldName), LiteralExpression(SyntaxKind.NullLiteralExpression))))));

            yield return PropertyDeclaration(PredefinedType(Token(SyntaxKind.StringKeyword)),
                    PasswordPropertyName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithExpressionBody(ArrowExpressionClause(IdentifierName(PasswordFieldName))),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, Block(
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(PasswordFieldName),
                            MethodHelpers.ArgumentOrThrowIfNull("value"))),
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(CacheFieldName), LiteralExpression(SyntaxKind.NullLiteralExpression))))));
        }

        protected override BlockSyntax GenerateApplyAsyncBody() => Block(
            MethodHelpers.ThrowIfArgumentNull(MessageParameterName),

            MethodHelpers.IfNull(IdentifierName(CacheFieldName), Block(
                ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(CacheFieldName),
                    WellKnownTypes.System.Convert.ToBase64String(WellKnownTypes.System.Text.Encoding.GetBytes(
                        WellKnownTypes.System.Text.Encoding.UTF8,
                        BinaryExpression(SyntaxKind.AddExpression,
                            BinaryExpression(SyntaxKind.AddExpression,
                                IdentifierName(UsernamePropertyName),
                                SyntaxHelpers.StringLiteral(":")),
                            IdentifierName(PasswordPropertyName))
                    ))))
            )),

            ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(MessageParameterName),
                        IdentifierName("Headers")),
                    IdentifierName("Authorization")),
                ObjectCreationExpression(WellKnownTypes.System.Net.Http.Headers.AuthenticationHeaderValue.Name)
                    .AddArgumentListArguments(
                        Argument(SyntaxHelpers.StringLiteral("Basic")),
                        Argument(IdentifierName(CacheFieldName))))),

            ReturnStatement(LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
    }
}

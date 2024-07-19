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
    public class ApiKeyQuerySecuritySchemeTypeGenerator(
        ILocatedOpenApiElement<OpenApiSecurityScheme> securitySchemeElement,
        GenerationContext context,
        IAuthenticationNamespace authenticationNamespace,
        ISerializationNamespace serializationNamespace)
        : SecuritySchemeTypeGenerator(securitySchemeElement, context, authenticationNamespace)
    {
        public const string DefaultApiKeyPropertyName = "ApiKey";

        private string? _apiKeyPropertyName;

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            _apiKeyPropertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property)
                .Format(GetClassName() != DefaultApiKeyPropertyName
                    ? DefaultApiKeyPropertyName
                    : DefaultApiKeyPropertyName + "Value");

            return base.Generate();
        }

        protected override IEnumerable<MemberDeclarationSyntax> GenerateAdditionalMembers(string className)
        {
            yield return ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithBody(Block());

            yield return PropertyDeclaration(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                    _apiKeyPropertyName!)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
        }

        protected override BlockSyntax GenerateApplyAsyncBody() => Block(
            MethodHelpers.ThrowIfArgumentNull(MessageParameterName),
            MethodHelpers.IfNotNull(IdentifierName(_apiKeyPropertyName!), Block(
                ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(MessageParameterName),
                        IdentifierName("RequestUri")),
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            serializationNamespace.QueryStringBuilder,
                            IdentifierName("AppendQueryParameter")),
                        ArgumentList(SeparatedList(
                        [
                            Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(MessageParameterName),
                                IdentifierName("RequestUri"))),
                            Argument(SyntaxHelpers.StringLiteral(SecurityScheme.Name)),
                            Argument(IdentifierName(_apiKeyPropertyName!))
                        ]))))))),
            ReturnStatement(LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
    }
}

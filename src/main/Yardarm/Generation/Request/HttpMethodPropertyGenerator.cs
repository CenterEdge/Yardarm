using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request;

internal class HttpMethodPropertyGenerator : IRequestMemberGenerator
{
    public const string MethodPropertyName = "Method";

    public IEnumerable<MemberDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
        ILocatedOpenApiElement<OpenApiMediaType>? mediaType) =>
        [
            PropertyDeclaration(
                attributeLists: default,
                TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)),
                WellKnownTypes.System.Net.Http.HttpMethod.Name,
                explicitInterfaceSpecifier: null,
                Identifier(MethodPropertyName),
                accessorList: null,
                ArrowExpressionClause(GetRequestMethod(operation)),
                initializer: null,
                Token(SyntaxKind.SemicolonToken))
        ];

    private static ExpressionSyntax GetRequestMethod(ILocatedOpenApiElement<OpenApiOperation> operation) =>
        operation.Key switch
        {
            "Delete" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Delete")),
            "Get" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Get")),
            "Head" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Head")),
            "Options" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Options")),
            "Post" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Post")),
            "Put" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Put")),
            "Trace" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Trace")),
            _ => ObjectCreationExpression(WellKnownTypes.System.Net.Http.HttpMethod.Name,
                ArgumentList(SingletonSeparatedList(
                    Argument(SyntaxHelpers.StringLiteral(operation.Key.ToUpperInvariant())))),
                initializer: null)
        };
}

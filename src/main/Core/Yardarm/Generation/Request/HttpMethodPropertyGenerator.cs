using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Packaging;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request;

internal class HttpMethodPropertyGenerator(GenerationContext context) : IRequestMemberGenerator
{
    public const string MethodPropertyName = "Method";

    public IEnumerable<MemberDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
        ILocatedOpenApiElement<IOpenApiMediaType>? mediaType) =>
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

    private ExpressionSyntax GetRequestMethod(ILocatedOpenApiElement<OpenApiOperation> operation) =>
        operation.Key switch
        {
            "CONNECT" when SupportsQueryAndConnect() =>
                QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Connect")),
            "DELETE" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Delete")),
            "GET" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Get")),
            "HEAD" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Head")),
            "OPTIONS" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Options")),
            "PATCH" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Patch")),
            "POST" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Post")),
            "PUT" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Put")),
            "QUERY" when SupportsQueryAndConnect() =>
                QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Query")),
            "TRACE" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Trace")),
            _ => ObjectCreationExpression(WellKnownTypes.System.Net.Http.HttpMethod.Name,
                ArgumentList(SingletonSeparatedList(
                    Argument(SyntaxHelpers.StringLiteral(operation.Key)))),
                initializer: null)
        };

    private bool SupportsQueryAndConnect() =>
        context.CurrentTargetFramework.Framework == NuGetFrameworkConstants.NetCoreApp &&
        context.CurrentTargetFramework.Version.Major >= 10;
}

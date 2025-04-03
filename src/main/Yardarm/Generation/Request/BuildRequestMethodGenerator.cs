using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request;

/// <summary>
/// Creates a legacy BuildRequest method for backward compatibility. This exists on each
/// request rather than on the inherited OperationRequest class so that it remains binary
/// compatible.
/// </summary>
public class BuildRequestMethodGenerator(
    ISerializationNamespace serializationNamespace,
    IRequestsNamespace requestsNamespace)
    : IRequestMemberGenerator
{
    public const string BuildRequestMethodName = "BuildRequest";
    protected const string RequestMessageVariableName = "requestMessage";

    private const string TypeSerializerRegistryParameterName = "typeSerializerRegistry";

    protected ISerializationNamespace SerializationNamespace { get; } = serializationNamespace;

    public IEnumerable<MemberDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
        ILocatedOpenApiElement<OpenApiMediaType>? mediaType)
    {
        if (mediaType is not null)
        {
            // Only generate for the main request, not media types
            return [];
        }

        return
        [
            MethodDeclaration(
                SingletonList(AttributeList(SingletonSeparatedList(
                    Attribute(
                        WellKnownTypes.System.ObsoleteAttribute.Name,
                        AttributeArgumentList(SingletonSeparatedList(
                            AttributeArgument(SyntaxHelpers.StringLiteral("Use the overload that accepts a BuildRequestContext.")))))))),
                TokenList(Token(SyntaxKind.PublicKeyword)),
                WellKnownTypes.System.Net.Http.HttpRequestMessage.Name,
                explicitInterfaceSpecifier: null,
                Identifier(BuildRequestMethodName),
                typeParameterList: default,
                ParameterList(SingletonSeparatedList(
                    Parameter(
                        attributeLists: default,
                        modifiers: default,
                        SerializationNamespace.ITypeSerializerRegistry,
                        Identifier(TypeSerializerRegistryParameterName),
                        @default: null))),
                constraintClauses: default,
                body: null,
                expressionBody: ArrowExpressionClause(InvocationExpression(
                IdentifierName(BuildRequestMethodName),
                ArgumentList(SingletonSeparatedList(
                    Argument(ObjectCreationExpression(
                        requestsNamespace.BuildRequestContext,
                        ArgumentList(SingletonSeparatedList(
                            Argument(IdentifierName(TypeSerializerRegistryParameterName)))),
                        initializer: null)))))))
        ];
    }

    public static InvocationExpressionSyntax InvokeBuildRequest(ExpressionSyntax requestInstance,
        ExpressionSyntax buildRequestContext) =>
        InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                requestInstance,
                IdentifierName(BuildRequestMethodName)),
            ArgumentList(SingletonSeparatedList(
                Argument(buildRequestContext))));
}

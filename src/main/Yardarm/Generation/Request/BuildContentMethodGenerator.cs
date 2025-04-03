using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request;

public class BuildContentMethodGenerator(
    IRequestsNamespace requestsNamespace,
    ISerializationNamespace serializationNamespace,
    IMediaTypeSelector mediaTypeSelector)
    : IBuildContentMethodGenerator
{
    public const string BuildContentMethodName = "BuildContent";

    private const string ContextParameterName = "context";

    protected ISerializationNamespace SerializationNamespace { get; } = serializationNamespace;
    protected IMediaTypeSelector MediaTypeSelector { get; } = mediaTypeSelector;

    public IEnumerable<MemberDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
        ILocatedOpenApiElement<OpenApiMediaType>? mediaType)
    {
        if (mediaType == null)
        {
            // In the base request class which has no body
            return [];
        }

        return
        [
            MethodDeclaration(
                attributeLists: default,
                TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)),
                NullableType(WellKnownTypes.System.Net.Http.HttpContent.Name),
                explicitInterfaceSpecifier: null,
                Identifier(BuildContentMethodName),
                typeParameterList: default,
                ParameterList(SingletonSeparatedList(
                    Parameter(
                        attributeLists: default,
                        modifiers: default,
                        requestsNamespace.BuildRequestContext,
                        Identifier(ContextParameterName),
                        @default: null))),
                constraintClauses: default,
                Block(GenerateStatements(operation, mediaType)),
                expressionBody: null)
        ];
    }

    protected virtual IEnumerable<StatementSyntax> GenerateStatements(
        ILocatedOpenApiElement<OpenApiOperation> operation, ILocatedOpenApiElement<OpenApiMediaType> mediaType)
    {
        ExpressionSyntax serializationDataExpression =
            SerializationDataPropertyGenerator.GetSerializationData();

        var createContentExpression =
            InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SerializationNamespace.TypeSerializerRegistryExtensions,
                    IdentifierName("Serialize")))
                .AddArgumentListArguments(
                    Argument(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(ContextParameterName),
                        IdentifierName("TypeSerializerRegistry"))),
                    Argument(IdentifierName(RequestMediaTypeGenerator.BodyPropertyName)),
                    Argument(SyntaxHelpers.StringLiteral(mediaType.Key)),
                    Argument(serializationDataExpression));

        yield return ReturnStatement(ConditionalExpression(
            IsPatternExpression(
                IdentifierName(RequestMediaTypeGenerator.BodyPropertyName),
                ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
            LiteralExpression(SyntaxKind.NullLiteralExpression),
            createContentExpression));
    }

    public static InvocationExpressionSyntax InvokeBuildContent(ExpressionSyntax requestInstance,
        ExpressionSyntax contextInstance) =>
        InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                requestInstance,
                IdentifierName(BuildContentMethodName)))
            .AddArgumentListArguments(
                Argument(contextInstance));
}

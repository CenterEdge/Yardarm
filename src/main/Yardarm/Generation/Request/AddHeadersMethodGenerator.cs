using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request;

public class AddHeadersMethodGenerator(
    IRequestsNamespace requestsNamespace,
    IMediaTypeSelector mediaTypeSelector,
    INameFormatterSelector nameFormatterSelector,
    ISerializationNamespace serializationNamespace)
    : IRequestMemberGenerator
{
    public const string AddHeadersMethodName = "AddHeaders";
    public const string ContextParameterName = "context";
    public const string RequestMessageParameterName = "requestMessage";

    protected IMediaTypeSelector MediaTypeSelector { get; } = mediaTypeSelector;
    protected INameFormatterSelector NameFormatterSelector { get; } = nameFormatterSelector;
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
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)),
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                explicitInterfaceSpecifier: null,
                Identifier(AddHeadersMethodName),
                typeParameterList: default,
                ParameterList(SeparatedList(
                [
                    Parameter(
                        attributeLists: default,
                        modifiers: default,
                        requestsNamespace.BuildRequestContext,
                        Identifier(ContextParameterName),
                        @default: null),
                    Parameter(
                        attributeLists: default,
                        modifiers: default,
                        WellKnownTypes.System.Net.Http.HttpRequestMessage.Name,
                        Identifier(RequestMessageParameterName),
                        @default: null)
                ])),
                constraintClauses: default,
                Block(GenerateStatements(operation)),
                expressionBody: null),
        ];
    }

    protected virtual IEnumerable<StatementSyntax> GenerateStatements(
        ILocatedOpenApiElement<OpenApiOperation> operation)
    {
        ILocatedOpenApiElement<OpenApiResponses> responseSet = operation.GetResponseSet();
        ILocatedOpenApiElement<OpenApiResponse> primaryResponse = responseSet
            .GetResponses()
            .OrderBy(p => p.Key)
            .First();

        ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(primaryResponse);
        if (mediaType != null)
        {
            yield return ExpressionStatement(InvocationExpression(
                    SyntaxHelpers.MemberAccess(RequestMessageParameterName, "Headers", "Accept", "Add"))
                .AddArgumentListArguments(
                    Argument(ObjectCreationExpression(WellKnownTypes.System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Name)
                        .AddArgumentListArguments(
                            Argument(SyntaxHelpers.StringLiteral(mediaType.Key))))));
        }

        INameFormatter propertyNameFormatter = NameFormatterSelector.GetFormatter(NameKind.Property);
        foreach (var headerParameter in operation.GetAllParameters()
            .Where(p => p.Element.In == ParameterLocation.Header)
            .Select(p => p.Element))
        {
            string propertyName = propertyNameFormatter.Format(headerParameter.Name);

            ExpressionSyntax valueExpression;
            if (headerParameter is {Schema.Type: "array"})
            {
                valueExpression = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.HeaderSerializer,
                        IdentifierName("SerializeList")),
                    ArgumentList(SeparatedList(
                    [
                        Argument(IdentifierName(propertyName)),
                        Argument(SyntaxHelpers.StringLiteral(headerParameter.Schema.Format))
                    ])));
            }
            else
            {
                valueExpression = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.HeaderSerializer,
                        IdentifierName("SerializePrimitive")),
                    ArgumentList(SeparatedList(
                    [
                        Argument(IdentifierName(propertyName)),
                        Argument(SyntaxHelpers.StringLiteral(headerParameter.Schema.Format))
                    ])));
            }

            StatementSyntax statement = ExpressionStatement(InvocationExpression(
                    SyntaxHelpers.MemberAccess(RequestMessageParameterName, "Headers", "Add"))
                .AddArgumentListArguments(
                    Argument(SyntaxHelpers.StringLiteral(headerParameter.Name)),
                    Argument(valueExpression)));

            if (!headerParameter.Required)
            {
                statement = MethodHelpers.IfNotNull(
                    IdentifierName(propertyName),
                    Block(statement));
            }

            yield return statement;
        }
    }

    public static InvocationExpressionSyntax InvokeAddHeaders(ExpressionSyntax requestInstance,
        ExpressionSyntax contextInstance,
        ExpressionSyntax requestMessageInstance) =>
        InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    requestInstance,
                    IdentifierName(AddHeadersMethodName)))
            .AddArgumentListArguments(Argument(contextInstance), Argument(requestMessageInstance));
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using NuGet.Common;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class AddHeadersMethodGenerator : IRequestMemberGenerator
    {
        public const string AddHeadersMethodName = "AddHeaders";
        public const string RequestMessageParameterName = "requestMessage";

        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected INameFormatterSelector NameFormatterSelector { get; }
        protected ISerializationNamespace SerializationNamespace { get; }

        public AddHeadersMethodGenerator(IMediaTypeSelector mediaTypeSelector, INameFormatterSelector nameFormatterSelector,
            ISerializationNamespace serializationNamespace)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            NameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            SerializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public MethodDeclarationSyntax GenerateHeader(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    AddHeadersMethodName)
                .AddParameterListParameters(
                    Parameter(Identifier(RequestMessageParameterName))
                        .WithType(WellKnownTypes.System.Net.Http.HttpRequestMessage.Name));

        public MemberDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType) =>
            GenerateHeader(operation)
                .AddModifiers(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.VirtualKeyword))
                .WithBody(Block(GenerateStatements(operation)));

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

            var propertyNameFormatter = NameFormatterSelector.GetFormatter(NameKind.Property);
            foreach (var headerParameter in operation.Element.Parameters.Where(p => p.In == ParameterLocation.Header))
            {
                string propertyName = propertyNameFormatter.Format(headerParameter.Name);

                StatementSyntax statement = ExpressionStatement(InvocationExpression(
                        SyntaxHelpers.MemberAccess(RequestMessageParameterName, "Headers", "Add"))
                    .AddArgumentListArguments(
                        Argument(SyntaxHelpers.StringLiteral(headerParameter.Name)),
                        Argument(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SerializationNamespace.HeaderSerializerInstance,
                            IdentifierName("Serialize")))
                            .AddArgumentListArguments(
                                Argument(IdentifierName(propertyName)),
                                Argument(headerParameter.Explode
                                    ? LiteralExpression(SyntaxKind.TrueLiteralExpression)
                                    : LiteralExpression(SyntaxKind.FalseLiteralExpression))))));

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
            ExpressionSyntax requestMessageInstance) =>
            InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        requestInstance,
                        IdentifierName(AddHeadersMethodName)))
                .AddArgumentListArguments(Argument(requestMessageInstance));
    }
}

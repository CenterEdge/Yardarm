using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Request;
using Yardarm.Generation.Tag;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Operation
{
    public class OperationMethodGenerator : IOperationMethodGenerator
    {
        public const string RequestParameterName = "request";
        protected const string RequestMessageVariableName = "requestMessage";

        protected GenerationContext Context { get; }
        protected IResponsesNamespace ResponsesNamespace { get; }

        public OperationMethodGenerator(GenerationContext context, IResponsesNamespace responsesNamespace)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ResponsesNamespace = responsesNamespace ?? throw new ArgumentNullException(nameof(responsesNamespace));
        }

        public BlockSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            Block(GenerateStatements(operation));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            yield return MethodHelpers.ThrowIfArgumentNull(RequestParameterName);

            yield return GenerateRequestMessageVariable(operation);

            yield return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .AddVariables(VariableDeclarator("responseMessage")
                    .WithInitializer(EqualsValueClause(
                        SyntaxHelpers.AwaitConfiguredFalse(InvocationExpression(
                                SyntaxHelpers.MemberAccess(TagTypeGenerator.HttpClientFieldName, "SendAsync"))
                            .AddArgumentListArguments(
                                Argument(IdentifierName(RequestMessageVariableName)),
                                Argument(IdentifierName(MethodHelpers.CancellationTokenParameterName))))))));

            yield return ReturnStatement(GenerateResponse(operation, IdentifierName("responseMessage")));
        }

        protected virtual StatementSyntax GenerateRequestMessageVariable(
            ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodHelpers.LocalVariableDeclarationWithInitializer(RequestMessageVariableName,
                BuildRequestMethodGenerator.InvokeBuildRequest(
                    IdentifierName(RequestParameterName),
                    IdentifierName(TagTypeGenerator.TypeSerializerRegistryFieldName)));

        protected virtual ExpressionSyntax GenerateResponse(
            ILocatedOpenApiElement<OpenApiOperation> operation, ExpressionSyntax responseMessage) =>
            SwitchExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    responseMessage,
                    IdentifierName("StatusCode")),
                SeparatedList(operation
                    .GetResponseSet()
                    .GetResponses()
                    .Select(p => SwitchExpressionArm(
                        ConstantPattern(ParseStatusCode(p.Key)),
                        ObjectCreationExpression(
                                Context.TypeNameProvider.GetName(p))
                            .AddArgumentListArguments(
                                Argument(IdentifierName("responseMessage")),
                                Argument(IdentifierName(TagTypeGenerator.TypeSerializerRegistryFieldName)))))))
                .AddArms(SwitchExpressionArm(DiscardPattern(),
                    ThrowExpression(ObjectCreationExpression(ResponsesNamespace.UnknownStatusCodeException)
                        .AddArgumentListArguments(
                            Argument(IdentifierName("responseMessage"))))));

        [Pure]
        private static ExpressionSyntax ParseStatusCode(string statusCodeStr) =>
            Enum.TryParse(statusCodeStr, out HttpStatusCode statusCode)
                ? (ExpressionSyntax)MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    WellKnownTypes.System.Net.HttpStatusCode.Name,
                    IdentifierName(statusCode.ToString()))
                : CastExpression(
                    WellKnownTypes.System.Net.HttpStatusCode.Name,
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(int.Parse(statusCodeStr))));
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
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

        protected const string AuthenticatorVariableName = "authenticator";
        protected const string RequestMessageVariableName = "requestMessage";

        protected GenerationContext Context { get; }
        protected IRequestsNamespace RequestsNamespace { get; }
        protected IResponsesNamespace ResponsesNamespace { get; }

        public OperationMethodGenerator(GenerationContext context, IRequestsNamespace requestsNamespace, IResponsesNamespace responsesNamespace)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(requestsNamespace);
            ArgumentNullException.ThrowIfNull(responsesNamespace);

            Context = context;
            RequestsNamespace = requestsNamespace;
            ResponsesNamespace = responsesNamespace;
        }

        public BlockSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            Block(GenerateStatements(operation));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            yield return MethodHelpers.ThrowIfArgumentNull(RequestParameterName);

            yield return GenerateAuthenticatorVariable();

            yield return GenerateRequestMessageVariable(operation);

            yield return MethodHelpers.IfNotNull(
                IdentifierName(AuthenticatorVariableName),
                Block(ExpressionStatement(SyntaxHelpers.AwaitConfiguredFalse(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(AuthenticatorVariableName),
                            IdentifierName("ApplyAsync")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName(RequestMessageVariableName)),
                        Argument(IdentifierName(MethodHelpers.CancellationTokenParameterName)))))));

            yield return LocalDeclarationStatement(VariableDeclaration(
                IdentifierName("var"),
                SingletonSeparatedList(VariableDeclarator(
                    Identifier("responseMessage"),
                    null,
                    EqualsValueClause(
                        SyntaxHelpers.AwaitConfiguredFalse(InvocationExpression(
                            SyntaxHelpers.MemberAccess(TagImplementationTypeGenerator.HttpClientFieldName, "SendAsync"),
                            ArgumentList(SeparatedList(new[]
                            {
                                Argument(IdentifierName(RequestMessageVariableName)),
                                Argument(ConditionalExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(RequestParameterName),
                                        IdentifierName("EnableResponseStreaming")),
                                    WellKnownTypes.System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                                    WellKnownTypes.System.Net.Http.HttpCompletionOption.ResponseContentRead)),
                                Argument(IdentifierName(MethodHelpers.CancellationTokenParameterName))
                            })))))))));

            // If there is an exception handling the response before we return, for example in the authenticator or in header parsing,
            // it's important that we dispose the response in the case of HttpCompletionOption.ResponseHeadersRead. Wrap in a
            // try..catch with a rethrow to ensure that happens.

            yield return TryStatement(
                Block(
                    MethodHelpers.IfNotNull(
                        IdentifierName(AuthenticatorVariableName),
                        Block(ExpressionStatement(SyntaxHelpers.AwaitConfiguredFalse(InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(AuthenticatorVariableName),
                                    IdentifierName("ProcessResponseAsync")),
                                ArgumentList(SeparatedList(new[] {
                                    Argument(IdentifierName("responseMessage")),
                                    Argument(IdentifierName(MethodHelpers.CancellationTokenParameterName))
                                }))))))),
                    ReturnStatement(GenerateResponse(operation, IdentifierName("responseMessage")))
                ),
                SingletonList(CatchClause(
                    null,
                    null,
                    Block(
                        ExpressionStatement(InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("responseMessage"),
                                IdentifierName("Dispose")))),
                        ThrowStatement()
                    ))),
                null);
        }

        protected virtual StatementSyntax GenerateAuthenticatorVariable() =>
            MethodHelpers.LocalVariableDeclarationWithInitializer(AuthenticatorVariableName,
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(TagImplementationTypeGenerator.AuthenticatorsFieldName),
                        IdentifierName("SelectAuthenticator")))
                    .AddArgumentListArguments(
                        Argument(IdentifierName(RequestParameterName))));

        protected virtual StatementSyntax GenerateRequestMessageVariable(
            ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodHelpers.LocalVariableDeclarationWithInitializer(RequestMessageVariableName,
                BuildRequestMethodGenerator.InvokeBuildRequest(
                    IdentifierName(RequestParameterName),
                    IdentifierName(TagImplementationTypeGenerator.TypeSerializerRegistryFieldName)));

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
                                Context.TypeGeneratorRegistry.Get(p).TypeInfo.Name)
                            .AddArgumentListArguments(
                                Argument(IdentifierName("responseMessage")),
                                Argument(IdentifierName(TagImplementationTypeGenerator.TypeSerializerRegistryFieldName)))))))
                .AddArms(SwitchExpressionArm(DiscardPattern(),
                    ObjectCreationExpression(
                        Context.TypeGeneratorRegistry.Get(operation.GetResponseSet().GetUnknownResponse()).TypeInfo.Name)
                        .AddArgumentListArguments(
                            Argument(IdentifierName("responseMessage")),
                            Argument(IdentifierName(TagImplementationTypeGenerator.TypeSerializerRegistryFieldName)))));

        [Pure]
        private static ExpressionSyntax ParseStatusCode(string statusCodeStr) =>
            // The HttpStatusCode enum available in .NET Core 3.1 used by Yardarm has more values in it than .NET Standard 2.0
            // for the compiled SDK, so if the spec has any new status codes (i.e. 207) it will cause compilation errors.
            // Instead cast the numeric value.
            CastExpression(
                WellKnownTypes.System.Net.HttpStatusCode.Name,
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(int.Parse(statusCodeStr))));
    }
}

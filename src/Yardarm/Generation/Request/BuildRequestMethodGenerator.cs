using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class BuildRequestMethodGenerator : IBuildRequestMethodGenerator
    {
        public const string BuildRequestMethodName = "BuildRequest";
        protected const string RequestMessageVariableName = "requestMessage";

        private const string TypeSerializerRegistryParameterName = "typeSerializerRegistry";

        protected ISerializationNamespace SerializationNamespace { get; }

        public BuildRequestMethodGenerator(ISerializationNamespace serializationNamespace)
        {
            SerializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public MethodDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodDeclaration(
                    WellKnownTypes.System.Net.Http.HttpRequestMessage.Name,
        BuildRequestMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier(TypeSerializerRegistryParameterName))
                        .WithType(SerializationNamespace.ITypeSerializerRegistry))
                .WithBody(Block(GenerateStatements(operation)));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            yield return GenerateRequestMessageVariable(operation);

            yield return ExpressionStatement(AddHeadersMethodGenerator.InvokeAddHeaders(
                ThisExpression(),
                IdentifierName(RequestMessageVariableName)));

            yield return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SyntaxHelpers.MemberAccess(RequestMessageVariableName, "Content"),
                BuildContentMethodGenerator.InvokeBuildContent(
                    ThisExpression(),
                    IdentifierName(TypeSerializerRegistryParameterName))));

            yield return ReturnStatement(IdentifierName(RequestMessageVariableName));
        }

        protected virtual StatementSyntax GenerateRequestMessageVariable(
            ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodHelpers.LocalVariableDeclarationWithInitializer(RequestMessageVariableName,
                ObjectCreationExpression(WellKnownTypes.System.Net.Http.HttpRequestMessage.Name)
                    .AddArgumentListArguments(
                        Argument(GetRequestMethod(operation)),
                        Argument(BuildUriMethodGenerator.InvokeBuildUri(ThisExpression()))));

        protected virtual ExpressionSyntax GetRequestMethod(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Key switch
            {
                "Delete" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Delete")),
                "Get" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Get")),
                "Head" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Head")),
                "Options" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Options")),
                "Post" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Post")),
                "Put" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Put")),
                "Trace" => QualifiedName(WellKnownTypes.System.Net.Http.HttpMethod.Name, IdentifierName("Trace")),
                _ => ObjectCreationExpression(WellKnownTypes.System.Net.Http.HttpMethod.Name).AddArgumentListArguments(
                    Argument(SyntaxHelpers.StringLiteral(operation.Key.ToUpperInvariant())))
            };

        public static InvocationExpressionSyntax InvokeBuildRequest(ExpressionSyntax requestInstance,
            ExpressionSyntax typeSerializerRegistry) =>
            InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        requestInstance,
                        IdentifierName(BuildRequestMethodName)))
                .AddArgumentListArguments(
                    Argument(typeSerializerRegistry));
    }
}

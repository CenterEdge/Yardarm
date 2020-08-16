using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Request;
using Yardarm.Generation.Tag;
using Yardarm.Helpers;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Operation
{
    public class OperationMethodGenerator : IOperationMethodGenerator
    {
        public const string RequestParameterName = "request";
        protected const string RequestMessageVariableName = "requestMessage";

        protected GenerationContext Context { get; }

        public OperationMethodGenerator(GenerationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BlockSyntax Generate(LocatedOpenApiElement<OpenApiOperation> operation) =>
            Block(GenerateStatements(operation));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(LocatedOpenApiElement<OpenApiOperation> operation)
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

            // Placeholder until we actually do the request
            yield return ThrowStatement(ObjectCreationExpression(
                        QualifiedName(IdentifierName("System"), IdentifierName("NotImplementedException")))
                    .WithArgumentList(ArgumentList()))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        protected virtual StatementSyntax GenerateRequestMessageVariable(
            LocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodHelpers.LocalVariableDeclarationWithInitializer(RequestMessageVariableName,
                BuildRequestMethodGenerator.InvokeBuildRequest(
                    IdentifierName(RequestParameterName)));
    }
}

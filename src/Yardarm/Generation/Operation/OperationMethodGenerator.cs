using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Tag;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using Yardarm.Spec.Path;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Operation
{
    public class OperationMethodGenerator : IOperationMethodGenerator
    {
        public const string UrlVariableName = "url";
        public const string RequestMessageVariableName = "requestMessage";

        private readonly IMediaTypeSelector _mediaTypeSelector;

        protected GenerationContext Context { get; }

        public OperationMethodGenerator(GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            _mediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BlockSyntax Generate(LocatedOpenApiElement<OpenApiOperation> operation) =>
            Block(GenerateStatements(operation));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(LocatedOpenApiElement<OpenApiOperation> operation)
        {
            yield return GenerateUrlVariable(operation);

            yield return GenerateRequestMessageVariable(operation);

            foreach (var header in GenerateHeaderConfiguration(operation))
            {
                yield return header;
            }

            // Placeholder until we actually do the request
            yield return ThrowStatement(ObjectCreationExpression(
                        QualifiedName(IdentifierName("System"), IdentifierName("NotImplementedException")))
                    .WithArgumentList(ArgumentList()))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        protected virtual StatementSyntax GenerateUrlVariable(LocatedOpenApiElement<OpenApiOperation> operation)
        {
            var propertyNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            var path = (LocatedOpenApiElement<OpenApiPathItem>)operation.Parents[0];

            return MethodHelpers.LocalVariableDeclarationWithInitializer(UrlVariableName,
                PathParser.Parse(path.Key).ToInterpolatedStringExpression(parameterName =>
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(TagTypeGenerator.RequestParameterName),
                        IdentifierName(propertyNameFormatter.Format(parameterName)))));
        }

        protected virtual StatementSyntax GenerateRequestMessageVariable(
            LocatedOpenApiElement<OpenApiOperation> operation)
        {
            return MethodHelpers.LocalVariableDeclarationWithInitializer(RequestMessageVariableName,
                ObjectCreationExpression(WellKnownTypes.HttpRequestMessage())
                    .AddArgumentListArguments(Argument(GetRequestMethod(operation)),
                        Argument(IdentifierName(UrlVariableName))));
        }

        protected virtual IEnumerable<StatementSyntax> GenerateHeaderConfiguration(
            LocatedOpenApiElement<OpenApiOperation> operation)
        {
            LocatedOpenApiElement<OpenApiResponses> responseSet = operation.GetResponseSet();
            LocatedOpenApiElement<OpenApiResponse> primaryResponse = responseSet.Element
                .OrderBy(p => p.Key)
                .Select(p => responseSet.CreateChild(p.Value, p.Key))
                .First();

            LocatedOpenApiElement<OpenApiMediaType>? mediaType = _mediaTypeSelector.Select(primaryResponse);
            if (mediaType != null)
            {
                yield return ExpressionStatement(InvocationExpression(
                        SyntaxHelpers.MemberAccess(RequestMessageVariableName, "Headers", "Accept", "Add"))
                    .AddArgumentListArguments(
                        Argument(ObjectCreationExpression(WellKnownTypes.MediaTypeWithQualityHeaderValue())
                            .AddArgumentListArguments(
                                Argument(SyntaxHelpers.StringLiteral(mediaType.Key))))));
            }

            var propertyNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);
            foreach (var headerParameter in operation.Element.Parameters.Where(p => p.In == ParameterLocation.Header))
            {
                string propertyName = propertyNameFormatter.Format(headerParameter.Name);

                StatementSyntax statement = ExpressionStatement(InvocationExpression(
                        SyntaxHelpers.MemberAccess(RequestMessageVariableName, "Headers", "Add"))
                    .AddArgumentListArguments(
                        Argument(SyntaxHelpers.StringLiteral(headerParameter.Name)),
                        Argument(InvocationExpression(
                            SyntaxHelpers.MemberAccess(TagTypeGenerator.RequestParameterName, propertyName,
                                "ToString")))));

                if (!headerParameter.Required)
                {
                    statement = MethodHelpers.IfNotNull(
                        SyntaxHelpers.MemberAccess(TagTypeGenerator.RequestParameterName, propertyName),
                        Block(statement));
                }

                yield return statement;
            }
        }

        protected virtual ExpressionSyntax GetRequestMethod(LocatedOpenApiElement<OpenApiOperation> operation) =>
            operation.Key switch
            {
                "Delete" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Delete")),
                "Get" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Get")),
                "Head" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Head")),
                "Options" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Options")),
                "Patch" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Patch")),
                "Post" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Post")),
                "Put" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Put")),
                "Trace" => QualifiedName(WellKnownTypes.HttpMethod(), IdentifierName("Trace")),
                _ => ObjectCreationExpression(WellKnownTypes.HttpMethod()).AddArgumentListArguments(
                    Argument(SyntaxHelpers.StringLiteral(operation.Key.ToUpperInvariant())))
            };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using Yardarm.Spec.Path;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class BuildUriMethodGenerator : IRequestMemberGenerator
    {
        public const string BuildUriMethodName = "BuildUri";

        protected GenerationContext Context { get; }
        protected ISerializationNamespace SerializationNamespace { get; }

        public BuildUriMethodGenerator(GenerationContext context, ISerializationNamespace serializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(serializationNamespace);

            Context = context;
            SerializationNamespace = serializationNamespace;
        }

        public MethodDeclarationSyntax GenerateHeader(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            MethodDeclaration(
                PredefinedType(Token(SyntaxKind.StringKeyword)),
                BuildUriMethodName);

        public IEnumerable<MemberDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiOperation> operation,
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType)
        {
            if (mediaType is not null)
            {
                // Only generate for the main request, not media types
                yield break;
            }

            yield return GenerateHeader(operation)
                .AddModifiers(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.VirtualKeyword))
                .WithBody(Block(GenerateBody(operation, mediaType)));
        }

        public IEnumerable<StatementSyntax> GenerateBody(ILocatedOpenApiElement<OpenApiOperation> operation,
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType)
        {
            var propertyNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            var path = (LocatedOpenApiElement<OpenApiPathItem>)operation.Parent!;

            var allParameters = operation.GetAllParameters()
                .Select(p => p.Element)
                .ToDictionary(p => p.Name, p => p);

            ExpressionSyntax pathExpression = PathParser.Parse(path.Key).ToInterpolatedStringExpression(
                pathSegment =>
                {
                    allParameters.TryGetValue(pathSegment.Value, out var parameter);

                    if (parameter?.Schema?.Type == "array")
                    {
                        return InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SerializationNamespace.PathSegmentSerializer,
                                IdentifierName("SerializeList")),
                            ArgumentList(SeparatedList([
                                Argument(SyntaxHelpers.StringLiteral(pathSegment.Value)),
                                Argument(IdentifierName(propertyNameFormatter.Format(pathSegment.Value))),
                                Argument(GetStyleExpression(parameter)),
                                Argument(GetExplodeExpression(parameter)),
                                Argument(SyntaxHelpers.StringLiteral(parameter.Schema?.Format))
                            ])));
                    }
                    else
                    {
                        return InvocationExpression(
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SerializationNamespace.PathSegmentSerializer,
                                IdentifierName("Serialize")),
                            ArgumentList(SeparatedList([
                                Argument(SyntaxHelpers.StringLiteral(pathSegment.Value)),
                                Argument(IdentifierName(propertyNameFormatter.Format(pathSegment.Value))),
                                Argument(GetStyleExpression(parameter)),
                                Argument(SyntaxHelpers.StringLiteral(parameter?.Schema?.Format))
                            ])));
                    }
                });

            OpenApiParameter[] queryParameters = allParameters.Values
                .Where(p => (p.In ?? ParameterLocation.Query) == ParameterLocation.Query)
                .ToArray();

            if (queryParameters.Length > 0)
            {
                const string queryStringBuilderVariableName = "queryStringBuilder";

                yield return LocalDeclarationStatement(VariableDeclaration(SerializationNamespace.QueryStringBuilder,
                    SingletonSeparatedList(VariableDeclarator(
                        Identifier(queryStringBuilderVariableName),
                        null,
                        EqualsValueClause(ImplicitObjectCreationExpression(
                            Token(SyntaxKind.NewKeyword),
                            ArgumentList(SingletonSeparatedList(Argument(pathExpression))),
                            null))))));

                foreach (var queryParameter in queryParameters)
                {
                    if (queryParameter.Schema.Type == "array")
                    {
                        yield return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(queryStringBuilderVariableName),
                                IdentifierName("AppendList")),
                            ArgumentList(SeparatedList(new ArgumentSyntax[]
                            {
                                Argument(SyntaxHelpers.StringLiteral(Uri.EscapeDataString(queryParameter.Name))),
                                Argument(IdentifierName(propertyNameFormatter.Format(queryParameter.Name))),
                                Argument(LiteralExpression(queryParameter.Explode ? SyntaxKind.TrueLiteralExpression: SyntaxKind.FalseLiteralExpression)),
                                Argument(queryParameter.Style switch
                                {
                                    ParameterStyle.SpaceDelimited => SyntaxHelpers.StringLiteral("%20"),
                                    ParameterStyle.PipeDelimited => SyntaxHelpers.StringLiteral("|"),
                                    _ => SyntaxHelpers.StringLiteral(",")
                                }),
                                Argument(LiteralExpression(queryParameter.AllowReserved ? SyntaxKind.TrueLiteralExpression: SyntaxKind.FalseLiteralExpression)),
                                Argument(SyntaxHelpers.StringLiteral(queryParameter.Schema.Format))
                            }))));
                    }
                    else
                    {
                        yield return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(queryStringBuilderVariableName),
                                IdentifierName("AppendPrimitive")),
                            ArgumentList(SeparatedList(new ArgumentSyntax[]
                            {
                                Argument(SyntaxHelpers.StringLiteral(Uri.EscapeDataString(queryParameter.Name))),
                                Argument(IdentifierName(propertyNameFormatter.Format(queryParameter.Name))),
                                Argument(LiteralExpression(queryParameter.AllowReserved ? SyntaxKind.TrueLiteralExpression: SyntaxKind.FalseLiteralExpression)),
                                Argument(SyntaxHelpers.StringLiteral(queryParameter.Schema.Format))
                            }))));
                    }
                }

                yield return ReturnStatement(
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(queryStringBuilderVariableName), IdentifierName("ToString"))));
            }
            else
            {
                yield return ReturnStatement(pathExpression);
            }
        }

        public static InvocationExpressionSyntax InvokeBuildUri(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    requestInstance,
                    IdentifierName(BuildUriMethodName)));

        protected ExpressionSyntax GetStyleExpression(OpenApiParameter? parameter) =>
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SerializationNamespace.PathSegmentStyle,
                parameter?.Style switch
                {
                    ParameterStyle.Label => IdentifierName("Label"),
                    ParameterStyle.Matrix => IdentifierName("Matrix"),
                    _ => IdentifierName("Simple")
                });

        protected ExpressionSyntax GetExplodeExpression(OpenApiParameter parameter) =>
            parameter.Explode
                ? LiteralExpression(SyntaxKind.TrueLiteralExpression)
                : LiteralExpression(SyntaxKind.FalseLiteralExpression);
    }
}

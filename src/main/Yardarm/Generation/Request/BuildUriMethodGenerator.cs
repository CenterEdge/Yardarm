﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using Yardarm.Spec.Path;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request;

public class BuildUriMethodGenerator(
    GenerationContext context,
    ISerializationNamespace serializationNamespace,
    IRequestsNamespace requestsNamespace)
    : IRequestMemberGenerator
{
    public const string BuildUriMethodName = "BuildUri";
    public const string ContextParameterName = "context";

    protected GenerationContext Context { get; } = context;
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
                WellKnownTypes.System.Uri.Name,
                explicitInterfaceSpecifier: null,
                Identifier(BuildUriMethodName),
                typeParameterList: default,
                ParameterList(SingletonSeparatedList(
                    Parameter(
                        attributeLists: default,
                        modifiers: default,
                        requestsNamespace.BuildRequestContext,
                        Identifier(ContextParameterName),
                        @default: null))),
                constraintClauses: default,
                Block(GenerateBody(operation, mediaType)),
                expressionBody: null)
        ];
    }

    private Func<PathSegment, InterpolationSyntax> CreateLegacyParameterInterpolationBuilder(
        Dictionary<string, OpenApiParameter> allParameters,
        INameFormatter propertyNameFormatter)
    {
        // Uses less performant string interpolation that generates intermediate strings
        // for target frameworks < .NET 6.0

        return pathSegment =>
        {
            allParameters.TryGetValue(pathSegment.Value, out var parameter);

            if (parameter?.Schema?.Type == "array")
            {
                return Interpolation(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.PathSegmentSerializer,
                        IdentifierName("SerializeList")),
                    ArgumentList(SeparatedList([
                        Argument(SyntaxHelpers.StringLiteral(pathSegment.Value)),
                        Argument(SyntaxHelpers.StringLiteral(pathSegment.Value)),
                        Argument(IdentifierName(propertyNameFormatter.Format(pathSegment.Value))),
                        Argument(GetStyleExpression(parameter)),
                        Argument(GetExplodeExpression(parameter)),
                        Argument(SyntaxHelpers.StringLiteral(parameter.Schema?.Format))
                    ]))));
            }
            else
            {
                return Interpolation(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.PathSegmentSerializer,
                        IdentifierName("Serialize")),
                    ArgumentList(SeparatedList([
                        Argument(SyntaxHelpers.StringLiteral(pathSegment.Value)),
                        Argument(IdentifierName(propertyNameFormatter.Format(pathSegment.Value))),
                        Argument(GetStyleExpression(parameter)),
                        Argument(SyntaxHelpers.StringLiteral(parameter?.Schema?.Format))
                    ]))));
            }
        };
    }

    private static Func<PathSegment, InterpolationSyntax> CreateModernParameterInterpolationBuilder(
        Dictionary<string, OpenApiParameter> allParameters,
        INameFormatter propertyNameFormatter)
    {
        // Uses more performant string interpolation, passing explode, style, and name via the
        // alignment and format values. See PathSegmentInterpolatedStringHandler for details.

        return pathSegment =>
        {
            allParameters.TryGetValue(pathSegment.Value, out var parameter);

            int alignment = 0;
            string? format = null;

            if (parameter is not null)
            {
                format = parameter.Schema?.Format;

                if (parameter.Style == ParameterStyle.Label)
                {
                    alignment = 1;
                }
                else if (parameter.Style == ParameterStyle.Matrix)
                {
                    alignment = 2 + pathSegment.Value.Length;
                    if (parameter.Explode)
                    {
                        alignment = -alignment;
                    }

                    format = $"{pathSegment.Value}{format}";
                }
            }

            return Interpolation(
                IdentifierName(propertyNameFormatter.Format(pathSegment.Value)),
                alignment != 0
                    ? InterpolationAlignmentClause(Token(SyntaxKind.CommaToken), LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(alignment)))
                    : null,
                format is not null
                    ? InterpolationFormatClause(Token(SyntaxKind.ColonToken), Token(default, SyntaxKind.InterpolatedStringTextToken, format, format, default))
                    : null);
        };
    }

    public IEnumerable<StatementSyntax> GenerateBody(ILocatedOpenApiElement<OpenApiOperation> operation,
        ILocatedOpenApiElement<OpenApiMediaType>? mediaType)
    {
        var propertyNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

        var path = (LocatedOpenApiElement<OpenApiPathItem>)operation.Parent!;

        var allParameters = operation.GetAllParameters()
            .Select(p => p.Element)
            .ToDictionary(p => p.Name, p => p);

        PathSegment[] parsedPath = PathParser.Parse(path.Key);

        ExpressionSyntax pathExpression;
        if (parsedPath is [] or [{ Type: PathSegmentType.Text }])
        {
            // No path parameters, just return the path, but use the parsed value that trims the leading slash
            pathExpression = SyntaxHelpers.StringLiteral(parsedPath.Length > 0 ? parsedPath[0].Value : "");
        }
        else if (Context.CurrentTargetFramework.Version.Major >= 6)
        {
            pathExpression = parsedPath.ToInterpolatedStringExpression(
                CreateModernParameterInterpolationBuilder(allParameters, propertyNameFormatter));

            // Yield a 256 character stackalloc for an initial buffer
            yield return LocalDeclarationStatement(VariableDeclaration(
                type: QualifiedName(
                    IdentifierName("System"),
                    GenericName(
                        Identifier("Span"),
                        TypeArgumentList(SingletonSeparatedList<TypeSyntax>(PredefinedType(Token(SyntaxKind.CharKeyword)))))),
                variables: SingletonSeparatedList(VariableDeclarator(
                    Identifier("initialBuffer"),
                    argumentList: null,
                    initializer: EqualsValueClause(
                        StackAllocArrayCreationExpression(
                            ArrayType(PredefinedType(Token(SyntaxKind.CharKeyword)))
                            .WithRankSpecifiers(SingletonList(
                            ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(256))
                            ))))))))));

            // Wrap in a call to PathSegmentSerializer.Serialize to ensure the handler is used
            pathExpression = InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SerializationNamespace.PathSegmentSerializer,
                    IdentifierName("Build")),
                ArgumentList(SeparatedList([
                    Argument(IdentifierName("initialBuffer")),
                    Argument(pathExpression)
                ])));
        }
        else
        {
            pathExpression = parsedPath.ToInterpolatedStringExpression(
                CreateLegacyParameterInterpolationBuilder(allParameters, propertyNameFormatter));
        }

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
                        ArgumentList(SingletonSeparatedList(
                            Argument(pathExpression))),
                        null))))));

            foreach (OpenApiParameter queryParameter in queryParameters)
            {
                if (queryParameter.Schema.Type == "array")
                {
                    yield return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(queryStringBuilderVariableName),
                            IdentifierName("AppendList")),
                        ArgumentList(SeparatedList(
                        [
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
                        ]))));
                }
                else
                {
                    yield return ExpressionStatement(InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(queryStringBuilderVariableName),
                            IdentifierName("AppendPrimitive")),
                        ArgumentList(SeparatedList(
                        [
                            Argument(SyntaxHelpers.StringLiteral(Uri.EscapeDataString(queryParameter.Name))),
                            Argument(IdentifierName(propertyNameFormatter.Format(queryParameter.Name))),
                            Argument(LiteralExpression(queryParameter.AllowReserved ? SyntaxKind.TrueLiteralExpression: SyntaxKind.FalseLiteralExpression)),
                            Argument(SyntaxHelpers.StringLiteral(queryParameter.Schema.Format))
                        ]))));
                }
            }

            yield return ReturnStatement(ObjectCreationExpression(
                WellKnownTypes.System.Uri.Name,
                ArgumentList(SeparatedList(
                [
                    Argument(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(queryStringBuilderVariableName), IdentifierName("ToString")))),
                    Argument(WellKnownTypes.System.UriKind.RelativeOrAbsolute),
                ])),
                initializer: null));
        }
        else
        {
            yield return ReturnStatement(ObjectCreationExpression(
                WellKnownTypes.System.Uri.Name,
                ArgumentList(SeparatedList(
                [
                    Argument(pathExpression),
                    Argument(WellKnownTypes.System.UriKind.RelativeOrAbsolute),
                ])),
                initializer: null));
        }
    }

    public static InvocationExpressionSyntax InvokeBuildUri(ExpressionSyntax requestInstance,
        ExpressionSyntax contextInstance) =>
        InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    requestInstance,
                    IdentifierName(BuildUriMethodName)))
            .AddArgumentListArguments(Argument(contextInstance));

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

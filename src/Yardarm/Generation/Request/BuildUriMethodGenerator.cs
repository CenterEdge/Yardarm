using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Tag;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using Yardarm.Spec.Path;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class BuildUriMethodGenerator : IBuildUriMethodGenerator
    {
        public const string BuildUriMethodName = "BuildUri";

        protected GenerationContext Context { get; }
        protected ISerializationNamespace SerializationNamespace { get; }

        public BuildUriMethodGenerator(GenerationContext context, ISerializationNamespace serializationNamespace)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            SerializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public MethodDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            var propertyNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            var path = (LocatedOpenApiElement<OpenApiPathItem>)operation.Parents[0];

            ExpressionSyntax pathExpression = PathParser.Parse(path.Key).ToInterpolatedStringExpression(
                pathSegment =>
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.PathSegmentSerializerInstance,
                        IdentifierName("Serialize")))
                        .AddArgumentListArguments(
                            Argument(SyntaxHelpers.StringLiteral(pathSegment.TrimmedName)),
                            Argument(IdentifierName(propertyNameFormatter.Format(pathSegment.TrimmedName))),
                            Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SerializationNamespace.PathSegmentStyle,
                                pathSegment.Style)),
                            Argument(pathSegment.Explode)));

            OpenApiParameter[] queryParameters = operation.Element.Parameters
                .Where(p => (p.In ?? ParameterLocation.Query) == ParameterLocation.Query)
                .ToArray();

            if (queryParameters.Length > 0)
            {
                NameSyntax keyValuePairType = WellKnownTypes.System.Collections.Generic.KeyValuePair.Name(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    PredefinedType(Token(SyntaxKind.ObjectKeyword)));

                ExpressionSyntax buildArrayExpression = ArrayCreationExpression(
                        ArrayType(keyValuePairType)
                            .AddRankSpecifiers(ArrayRankSpecifier().AddSizes(OmittedArraySizeExpression())))
                    .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SeparatedList<ExpressionSyntax>(queryParameters
                            .Select(parameter => ObjectCreationExpression(keyValuePairType)
                                .AddArgumentListArguments(
                                    Argument(SyntaxHelpers.StringLiteral(parameter.Name)),
                                    Argument(IdentifierName(propertyNameFormatter.Format(parameter.Name))))))));

                pathExpression = WellKnownTypes.Yardarm.Client.OperationHelpers.AddQueryParameters(
                    pathExpression, buildArrayExpression);
            }

            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    BuildUriMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(ArrowExpressionClause(
                    pathExpression));
        }

        public static InvocationExpressionSyntax InvokeBuildUri(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    requestInstance,
                    IdentifierName(BuildUriMethodName)));
    }
}

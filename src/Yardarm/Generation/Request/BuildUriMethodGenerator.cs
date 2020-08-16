using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Tag;
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

        public BuildUriMethodGenerator(GenerationContext context)
        {
            Context = context;
        }

        public MethodDeclarationSyntax Generate(LocatedOpenApiElement<OpenApiOperation> operation)
        {
            var propertyNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            var path = (LocatedOpenApiElement<OpenApiPathItem>)operation.Parents[0];

            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    BuildUriMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(ArrowExpressionClause(
                    PathParser.Parse(path.Key).ToInterpolatedStringExpression(parameterName =>
                        IdentifierName(propertyNameFormatter.Format(parameterName)))));
        }

        public static InvocationExpressionSyntax InvokeBuildUri(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    requestInstance,
                    IdentifierName(BuildUriMethodName)));
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Responses
{
    /// <summary>
    /// Adds extension methods to cast OpenApiResponses interfaces to specific response types.
    /// </summary>
    public class ResponseTypeCastExtensionEnricher(
        GenerationContext context,
        IResponsesNamespace responsesNamespace,
        IHttpResponseCodeNameProvider httpResponseCodeNameProvider,
        IOperationNameProvider operationNameProvider)
        : IOpenApiSyntaxNodeEnricher<CompilationUnitSyntax, OpenApiResponses>
    {
        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target,
            OpenApiEnrichmentContext<OpenApiResponses> context)
        {
            NamespaceDeclarationSyntax? ns = target.ChildNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (ns == null)
            {
                return target;
            }

            return target.ReplaceNode(ns, ns.AddMembers(GenerateExtensionClass(context.LocatedElement)));
        }

        private ClassDeclarationSyntax GenerateExtensionClass(ILocatedOpenApiElement<OpenApiResponses> responseSet)
        {
            var operation = (ILocatedOpenApiElement<OpenApiOperation>)responseSet.Parent!;

            var nameFormatter = context.NameFormatterSelector.GetFormatter(NameKind.Class);

            string? operationName = operationNameProvider.GetOperationName(operation);
            Debug.Assert(operationName is not null);

            return ClassDeclaration(nameFormatter.Format(operationName + "-ResponseExtensions"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .AddMembers(GenerateExtensions(responseSet).ToArray<MemberDeclarationSyntax>());
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateExtensions(
            ILocatedOpenApiElement<OpenApiResponses> responseSet)
        {
            var nameFormatter = context.NameFormatterSelector.GetFormatter(NameKind.Method);

            TypeSyntax interfaceTypeName = context.TypeGeneratorRegistry.Get(responseSet).TypeInfo.Name;

            foreach (var response in responseSet.GetResponses())
            {
                string responseCode = Enum.TryParse<HttpStatusCode>(response.Key, out var statusCode)
                    ? httpResponseCodeNameProvider.GetName(statusCode)
                    : response.Key;

                TypeSyntax typeName = context.TypeGeneratorRegistry.Get(response).TypeInfo.Name;

                yield return MethodDeclaration(typeName, nameFormatter.Format("As-" + responseCode))
                    .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                    .AddParameterListParameters(
                        Parameter(Identifier("response"))
                            .WithType(interfaceTypeName)
                            .AddModifiers(Token(SyntaxKind.ThisKeyword)))
                    .WithExpressionBody(ArrowExpressionClause(
                        BinaryExpression(SyntaxKind.CoalesceExpression,
                            BinaryExpression(SyntaxKind.AsExpression,
                                IdentifierName("response"),
                                typeName),
                            ThrowExpression(ObjectCreationExpression(responsesNamespace.StatusCodeMismatchException)
                                .AddArgumentListArguments(
                                    Argument(IdentifierName("response")),
                                    Argument(TypeOfExpression(typeName)))))))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
            }
        }
    }
}

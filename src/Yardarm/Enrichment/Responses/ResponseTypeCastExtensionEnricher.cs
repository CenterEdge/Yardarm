using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Responses
{
    /// <summary>
    /// Adds extension methods to cast OpenApiResponses interfaces to specific response types.
    /// </summary>
    public class ResponseTypeCastExtensionEnricher : IOpenApiSyntaxNodeEnricher<CompilationUnitSyntax, OpenApiResponses>
    {
        private readonly GenerationContext _context;
        private readonly IResponsesNamespace _responsesNamespace;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        public int Priority => 0;

        public ResponseTypeCastExtensionEnricher(GenerationContext context, IResponsesNamespace responsesNamespace,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _responsesNamespace = responsesNamespace ?? throw new ArgumentNullException(nameof(responsesNamespace));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ?? throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

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

            var nameFormatter = _context.NameFormatterSelector.GetFormatter(NameKind.Class);

            return ClassDeclaration(nameFormatter.Format(operation.Element.OperationId + "-ResponseExtensions"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .AddMembers(GenerateExtensions(responseSet).ToArray<MemberDeclarationSyntax>());
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateExtensions(
            ILocatedOpenApiElement<OpenApiResponses> responseSet)
        {
            var nameFormatter = _context.NameFormatterSelector.GetFormatter(NameKind.Method);

            TypeSyntax interfaceTypeName = _context.TypeGeneratorRegistry.Get(responseSet).TypeInfo.Name;

            foreach (var response in responseSet.GetResponses())
            {
                string responseCode = Enum.TryParse<HttpStatusCode>(response.Key, out var statusCode)
                    ? _httpResponseCodeNameProvider.GetName(statusCode)
                    : response.Key;

                TypeSyntax typeName = _context.TypeGeneratorRegistry.Get(response).TypeInfo.Name;

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
                            ThrowExpression(ObjectCreationExpression(_responsesNamespace.StatusCodeMismatchException)
                                .AddArgumentListArguments(
                                    Argument(IdentifierName("response")),
                                    Argument(TypeOfExpression(typeName)))))));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Responses
{
    public class HeaderParsingEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiResponse>
    {
        private readonly GenerationContext _context;
        private readonly ISerializationNamespace _serializationNamespace;

        public HeaderParsingEnricher(GenerationContext context, ISerializationNamespace serializationNamespace)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiResponse> context) =>
            IsBaseResponseClass(context.LocatedElement) && context.Element.Headers.Count > 0
                ? target.AddMembers(GenerateMethod(context.LocatedElement))
                : target;

        /// <summary>
        /// Determines if this response type is the one where primary work is done. This would be either a response
        /// in the components section, or a response within an operation which isn't a reference to the components
        /// section. Are reference to components section is implemented as inherited from the components section
        /// implementation, which doesn't require primary work implementation.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static bool IsBaseResponseClass(ILocatedOpenApiElement<OpenApiResponse> response) =>
            response.IsRoot() || response.Element.Reference == null;

        public MethodDeclarationSyntax GenerateMethod(ILocatedOpenApiElement<OpenApiResponse> response) =>
            MethodDeclaration(
                default,
                new SyntaxTokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)),
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                null,
                Identifier("ParseHeaders"),
                null,
                ParameterList(SingletonSeparatedList(
                    Parameter(
                        default,
                        default,
                        WellKnownTypes.System.Net.Http.Headers.HttpResponseHeaders.Name,
                        Identifier("headers"),
                        null))),
                default,
                Block(GenerateStatements(response)),
                null);

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            ILocatedOpenApiElement<OpenApiResponse> response)
        {
            var propertyNameFormatter = _context.NameFormatterSelector.GetFormatter(NameKind.Property);

            // Declare values variable to hold TryGetValue out results
            yield return LocalDeclarationStatement(VariableDeclaration(
                    WellKnownTypes.System.Collections.Generic.IEnumerableT.Name(
                        PredefinedType(Token(SyntaxKind.StringKeyword))).MakeNullable())
                .AddVariables(VariableDeclarator("values")));

            NameSyntax valuesName = IdentifierName("values");

            foreach (var header in response.GetHeaders())
            {
                ILocatedOpenApiElement<OpenApiSchema> schemaElement = header.GetSchemaOrDefault();

                TypeSyntax typeName = _context.TypeGeneratorRegistry.Get(schemaElement).TypeInfo.Name;

                InvocationExpressionSyntax deserializeExpression =
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            _serializationNamespace.HeaderSerializerInstance,
                            GenericName("Deserialize")
                                .AddTypeArgumentListArguments(typeName)))
                        .AddArgumentListArguments(
                            Argument(valuesName),
                            Argument(header.Element.Explode
                                ? LiteralExpression(SyntaxKind.TrueLiteralExpression)
                                : LiteralExpression(SyntaxKind.FalseLiteralExpression)));

                yield return IfStatement(
                    WellKnownTypes.System.Net.Http.Headers.HttpHeaders.TryGetValues(
                        IdentifierName("headers"),
                        SyntaxHelpers.StringLiteral(header.Key),
                        valuesName),
                    Block(
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(propertyNameFormatter.Format(header.Key)),
                            deserializeExpression))));
            }
        }
    }
}

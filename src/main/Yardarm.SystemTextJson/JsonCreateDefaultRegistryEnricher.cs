using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonCreateDefaultRegistryEnricher : ICreateDefaultRegistryEnricher
    {
        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

        public JsonCreateDefaultRegistryEnricher(IJsonSerializationNamespace jsonSerializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);

            _jsonSerializationNamespace = jsonSerializationNamespace;
        }

        public ExpressionSyntax Enrich(ExpressionSyntax target) =>
            // Don't use the Add<T> overload here because it will cause trimming to retain
            // all constructors. This will then cause IL2026 warnings if trimming is enabled.
            // Instead create a new instance directly using the default constructor and add it.
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    target.WithTrailingTrivia(TriviaList(CarriageReturnLineFeed, Whitespace("                "))),
                    IdentifierName("Add")),
                ArgumentList(SeparatedList(new[]
                {
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        _jsonSerializationNamespace.JsonTypeSerializer,
                        IdentifierName("SupportedMediaTypes"))),
                    Argument(ObjectCreationExpression(_jsonSerializationNamespace.JsonTypeSerializer,
                        ArgumentList(SingletonSeparatedList(
                            Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                QualifiedName(_jsonSerializationNamespace.Name, IdentifierName(JsonSerializerContextGenerator.TypeName)),
                                IdentifierName("Default"))))),
                            null))
                })));
    }
}

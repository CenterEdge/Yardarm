using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson
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
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    target.WithTrailingTrivia(TriviaList(CarriageReturnLineFeed, Whitespace("                "))),
                    GenericName(
                        Identifier("Add"),
                        TypeArgumentList(SingletonSeparatedList<TypeSyntax>(_jsonSerializationNamespace.JsonTypeSerializer)))),
                ArgumentList(SingletonSeparatedList(
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        _jsonSerializationNamespace.JsonTypeSerializer,
                        IdentifierName("SupportedMediaTypes"))))));
    }
}

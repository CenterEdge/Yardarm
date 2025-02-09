using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment.Registration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson
{
    public class JsonCreateDefaultRegistryEnricher : ReturnValueRegistrationEnricher, ICreateDefaultRegistryEnricher
    {
        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

        public JsonCreateDefaultRegistryEnricher(IJsonSerializationNamespace jsonSerializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);

            _jsonSerializationNamespace = jsonSerializationNamespace;
        }

        protected override ExpressionSyntax EnrichReturnValue(ExpressionSyntax expression) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    GenericName(
                        Identifier("Add"),
                        TypeArgumentList(SingletonSeparatedList<TypeSyntax>(_jsonSerializationNamespace.JsonTypeSerializer)))),
                ArgumentList(SingletonSeparatedList(
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        _jsonSerializationNamespace.JsonTypeSerializer,
                        IdentifierName("SupportedMediaTypes"))))));
    }
}

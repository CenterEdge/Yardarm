using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment.Registration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson;

public class JsonCreateDefaultRegistryEnricher : ReturnValueRegistrationEnricher, ICreateDefaultRegistryEnricher
{
    private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

    public JsonCreateDefaultRegistryEnricher(IJsonSerializationNamespace jsonSerializationNamespace)
    {
        ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);

        _jsonSerializationNamespace = jsonSerializationNamespace;
    }

    protected override ExpressionSyntax EnrichReturnValue(ExpressionSyntax target)
        => AddSerializer(
            AddSerializer(target, _jsonSerializationNamespace.JsonTypeSerializer),
            _jsonSerializationNamespace.JsonLinesTypeSerializer);

    // Don't use the Add<T> overload here because it will cause trimming to retain
    // all constructors. This will then cause IL2026 warnings if trimming is enabled.
    // Instead create a new instance using the CreateDefault static method and add it.
    private static InvocationExpressionSyntax AddSerializer(ExpressionSyntax target, NameSyntax serializer)
        => InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                target,
                IdentifierName("Add")),
            ArgumentList(SeparatedList(
            [
                Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    serializer,
                    IdentifierName("SupportedMediaTypes"))),
                Argument(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, serializer, IdentifierName("CreateDefault")),
                    ArgumentList()))
            ])));
}

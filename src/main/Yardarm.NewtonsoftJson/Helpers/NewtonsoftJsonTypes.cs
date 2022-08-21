using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson.Helpers
{
    internal static class NewtonsoftJsonTypes
    {
        public static NameSyntax NewtonsoftJson { get; } = QualifiedName(
            AliasQualifiedName(
                IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                IdentifierName("Newtonsoft")),
            IdentifierName("Json"));

        public static NameSyntax NewtonsoftJsonConverters { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("Converters"));

        public static NameSyntax NewtonsoftJsonLinq { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("Linq"));

        public static class NullValueHandling
        {
            public static NameSyntax Name { get; } = QualifiedName(
                NewtonsoftJson,
                IdentifierName("NullValueHandling"));

            public static MemberAccessExpressionSyntax Include { get; } = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                Name,
                IdentifierName("Include"));

            public static MemberAccessExpressionSyntax Ignore { get; } = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                Name,
                IdentifierName("Ignore"));
        }

        public static NameSyntax JsonExtensionDataAttributeName { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("JsonExtensionDataAttribute"));

        public static NameSyntax JsonIgnoreAttributeName { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("JsonIgnoreAttribute"));

        public static NameSyntax JsonPropertyAttributeName { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("JsonProperty"));

        public static NameSyntax JsonConverterAttributeName { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("JsonConverter"));

        public static NameSyntax JTokenName { get; } = QualifiedName(
            NewtonsoftJsonLinq,
            IdentifierName("JToken"));

        public static NameSyntax StringEnumConverterName { get; } = QualifiedName(
            NewtonsoftJsonConverters,
            IdentifierName("StringEnumConverter"));
    }
}

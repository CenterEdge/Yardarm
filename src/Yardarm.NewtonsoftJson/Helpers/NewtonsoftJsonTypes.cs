using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson.Helpers
{
    internal static class NewtonsoftJsonTypes
    {
        public static NameSyntax NewtonsoftJson { get; } = QualifiedName(
            IdentifierName("Newtonsoft"),
            IdentifierName("Json"));

        public static NameSyntax NewtonsoftJsonConverters { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("Converters"));

        public static NameSyntax JsonPropertyAttributeName { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("JsonProperty"));

        public static NameSyntax JsonConverterAttributeName { get; } = QualifiedName(
            NewtonsoftJson,
            IdentifierName("JsonConverter"));

        public static NameSyntax StringEnumConverterName { get; } = QualifiedName(
            NewtonsoftJsonConverters,
            IdentifierName("StringEnumConverter"));
    }
}

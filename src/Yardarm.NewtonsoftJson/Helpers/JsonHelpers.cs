using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;

namespace Yardarm.NewtonsoftJson.Helpers
{
    internal static class JsonHelpers
    {
        public static NameSyntax NewtonsoftJson() => SyntaxFactory.QualifiedName(
            SyntaxFactory.IdentifierName("Newtonsoft"),
            SyntaxFactory.IdentifierName("Json"));

        public static NameSyntax NewtonsoftJsonConverters() => SyntaxFactory.QualifiedName(
            NewtonsoftJson(),
            SyntaxFactory.IdentifierName("Converters"));

        public static NameSyntax JsonPropertyAttributeName() => SyntaxFactory.QualifiedName(
            NewtonsoftJson(),
            SyntaxFactory.IdentifierName("JsonProperty"));

        public static NameSyntax JsonConverterAttributeName() => SyntaxFactory.QualifiedName(
            NewtonsoftJson(),
            SyntaxFactory.IdentifierName("JsonConverter"));

        public static NameSyntax StringEnumConverterName() => SyntaxFactory.QualifiedName(
            NewtonsoftJsonConverters(),
            SyntaxFactory.IdentifierName("StringEnumConverter"));
    }
}

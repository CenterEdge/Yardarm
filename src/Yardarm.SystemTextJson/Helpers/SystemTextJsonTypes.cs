using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Helpers
{
    internal static class SystemTextJsonTypes
    {
        public static NameSyntax SystemTextJson { get; } = QualifiedName(
            QualifiedName(
                IdentifierName("System"),
                IdentifierName("Text")),
            IdentifierName("Json"));

        public static NameSyntax Serialization { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("Serialization"));

        public static NameSyntax JsonPropertyNameAttributeName { get; } = QualifiedName(
            Serialization,
            IdentifierName("JsonPropertyName"));

        public static NameSyntax JsonConverterAttributeName { get; } = QualifiedName(
            Serialization,
            IdentifierName("JsonConverterAttribute"));
    }
}

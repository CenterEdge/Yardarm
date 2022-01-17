using Microsoft.CodeAnalysis.CSharp;
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

        public static NameSyntax JsonSerializer { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("JsonSerializer"));

        public static NameSyntax JsonSerializerOptions { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("JsonSerializerOptions"));

        public static NameSyntax JsonException { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("JsonException"));

        public static class JsonTokenType
        {
            public static NameSyntax Name { get; } = QualifiedName(
                SystemTextJson,
                IdentifierName("JsonTokenType"));

            public static MemberAccessExpressionSyntax Null { get; } = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                Name,
                IdentifierName("Null"));

            public static MemberAccessExpressionSyntax PropertyName { get; } = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                Name,
                IdentifierName("PropertyName"));

            public static MemberAccessExpressionSyntax StartObject { get; } = MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                Name,
                IdentifierName("StartObject"));
        }

        public static class Nodes
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static NameSyntax Name { get; } = QualifiedName(
                SystemTextJson,
                IdentifierName("Nodes"));

            public static NameSyntax JsonNodeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonNode"));
        }

        public static class Serialization
        {
            public static NameSyntax Name { get; } = QualifiedName(
                SystemTextJson,
                IdentifierName("Serialization"));

            public static NameSyntax JsonExtensionDataAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonExtensionDataAttribute"));

            public static NameSyntax JsonIgnoreAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonIgnoreAttribute"));

            public static class JsonIgnoreCondition
            {
                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static NameSyntax Name { get; } = QualifiedName(
                    Serialization.Name,
                    IdentifierName("JsonIgnoreCondition"));

                public static MemberAccessExpressionSyntax Always { get; } = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Name,
                    IdentifierName("Always"));

                public static MemberAccessExpressionSyntax Never { get; } = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Name,
                    IdentifierName("Never"));

                public static MemberAccessExpressionSyntax WhenWritingDefault { get; } = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Name,
                    IdentifierName("WhenWritingDefault"));

                public static MemberAccessExpressionSyntax WhenWritingNull { get; } = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Name,
                    IdentifierName("WhenWritingNull"));
            }

            public static NameSyntax JsonPropertyNameAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonPropertyName"));

            public static NameSyntax JsonConverterAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonConverterAttribute"));

            public static TypeSyntax JsonConverterName(TypeSyntax t) =>
                QualifiedName(Name, GenericName(Identifier("JsonConverter"),
                    TypeArgumentList(SingletonSeparatedList(t))));
        }

        public static NameSyntax Utf8JsonReader { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("Utf8JsonReader"));

        public static NameSyntax Utf8JsonWriter { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("Utf8JsonWriter"));
    }
}

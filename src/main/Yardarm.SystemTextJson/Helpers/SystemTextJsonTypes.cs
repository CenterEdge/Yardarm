using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Helpers
{
    internal static class SystemTextJsonTypes
    {
        public static NameSyntax SystemTextJson { get; } = QualifiedName(
            QualifiedName(
                AliasQualifiedName(
                    IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                    IdentifierName("System")),
                IdentifierName("Text")),
            IdentifierName("Json"));

        public static NameSyntax JsonElement { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("JsonElement"));

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

            public static NameSyntax JsonObjectName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonObject"));
        }

        public static class Serialization
        {
            public static NameSyntax Name { get; } = QualifiedName(
                SystemTextJson,
                IdentifierName("Serialization"));

            public static NameSyntax JsonDerivedTypeAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonDerivedTypeAttribute"));

            public static NameSyntax JsonExtensionDataAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonExtensionDataAttribute"));

            public static NameSyntax JsonIgnoreAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonIgnoreAttribute"));

            public static class JsonNumberHandling
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    Serialization.Name,
                    IdentifierName("JsonNumberHandling"));

                public static MemberAccessExpressionSyntax AllowReadingFromString { get; } = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Name,
                    IdentifierName("AllowReadingFromString"));
            }

            public static NameSyntax JsonPolymorphicAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonPolymorphicAttribute"));

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

            public static NameSyntax JsonSerializableAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonSerializableAttribute"));

            public static NameSyntax JsonSerializerContextName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonSerializerContext"));

            public static class JsonSourceGenerationMode
            {
                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static NameSyntax Name { get; } = QualifiedName(
                    SystemTextJsonTypes.Serialization.Name,
                    IdentifierName("JsonSourceGenerationMode"));

                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static MemberAccessExpressionSyntax Metadata { get; } = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Name,
                    IdentifierName("Metadata"));
            }

            public static NameSyntax JsonSourceGenerationOptionsAttributeName { get; } = QualifiedName(
                Name,
                IdentifierName("JsonSourceGenerationOptionsAttribute"));

            public static TypeSyntax JsonStringEnumConverterName(TypeSyntax enumType) =>
                QualifiedName(Name, GenericName(Identifier("JsonStringEnumConverter"),
                    TypeArgumentList(SingletonSeparatedList(enumType))));
        }

        public static NameSyntax Utf8JsonReader { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("Utf8JsonReader"));

        public static NameSyntax Utf8JsonWriter { get; } = QualifiedName(
            SystemTextJson,
            IdentifierName("Utf8JsonWriter"));
    }
}

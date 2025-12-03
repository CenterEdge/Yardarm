using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NodaTime.Helpers;

internal static class NodaTimeTypes
{
    public static NameSyntax NodaTime { get; } =
        AliasQualifiedName(IdentifierName(Token(SyntaxKind.GlobalKeyword)), IdentifierName("NodaTime"));

    public static NameSyntax DateTimeZoneProviders { get; } =
        QualifiedName(NodaTime, IdentifierName("DateTimeZoneProviders"));

    public static NameSyntax Duration { get; } =
        QualifiedName(NodaTime, IdentifierName("Duration"));

    public static NameSyntax LocalDate { get; } =
        QualifiedName(NodaTime, IdentifierName("LocalDate"));

    public static NameSyntax LocalDateTime { get; } =
        QualifiedName(NodaTime, IdentifierName("LocalDateTime"));

    public static NameSyntax LocalTime { get; } =
        QualifiedName(NodaTime, IdentifierName("LocalTime"));

    public static NameSyntax OffsetDateTime { get; } =
        QualifiedName(NodaTime, IdentifierName("OffsetDateTime"));

    public static NameSyntax OffsetTime { get; } =
        QualifiedName(NodaTime, IdentifierName("OffsetTime"));

    public static NameSyntax Period { get; } =
        QualifiedName(NodaTime, IdentifierName("Period"));

    public static class Serialization
    {
        public static NameSyntax Name { get; } =
            QualifiedName(NodaTime, IdentifierName("Serialization"));

        public static class SystemTextJson
        {
            public static NameSyntax Name { get; } =
                QualifiedName(Serialization.Name, IdentifierName("SystemTextJson"));

            public static NameSyntax NodaTimeDefaultJsonConverterFactory { get; } =
                QualifiedName(Name, IdentifierName("NodaTimeDefaultJsonConverterFactory"));
        }

        public static class JsonNet
        {
            public static NameSyntax Name { get; } =
                QualifiedName(Serialization.Name, IdentifierName("JsonNet"));

            public static NameSyntax Extensions { get; } =
                QualifiedName(Name, IdentifierName("Extensions"));
        }
    }
}

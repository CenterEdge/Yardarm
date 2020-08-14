using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    // ReSharper disable InconsistentNaming
    public static class WellKnownTypes
    {
        public static NameSyntax CancellationToken() =>
            QualifiedName(
                QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Threading")
                ),
                IdentifierName("CancellationToken"));

        public static NameSyntax HttpResponseMessage() =>
            QualifiedName(
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("Net")),
                    IdentifierName("Http")),
                IdentifierName("HttpResponseMessage"));

        public static NameSyntax HttpStatusCode() =>
            QualifiedName(
                QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Net")
                ),
                IdentifierName("HttpStatusCode"));

        public static NameSyntax IDisposable() =>
            QualifiedName(IdentifierName("System"), IdentifierName("IDisposable"));

        public static NameSyntax ListT(TypeSyntax itemType) =>
            QualifiedName(
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("Collections")
                    ),
                    IdentifierName("Generic")),
                GenericName(
                        Identifier("List"),
                        TypeArgumentList(SingletonSeparatedList(itemType))));

        public static NameSyntax RequiredAttribute() =>
            QualifiedName(
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("ComponentModel")
                    ),
                    IdentifierName("DataAnnotations")),
                IdentifierName("Required"));

        public static NameSyntax TaskT(TypeSyntax resultType) =>
            QualifiedName(
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("Threading")
                    ),
                    IdentifierName("Tasks")),
                GenericName(
                    Identifier("Task"),
                    TypeArgumentList(SingletonSeparatedList(resultType))));
    }
}

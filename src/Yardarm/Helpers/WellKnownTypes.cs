using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    // ReSharper disable InconsistentNaming
    public static class WellKnownTypes
    {
        public static TypeSyntax CancellationToken() =>
            QualifiedName(
                QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Threading")
                ),
                IdentifierName("CancellationToken"));

        public static TypeSyntax HttpResponseMessage() =>
            QualifiedName(
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("Net")),
                    IdentifierName("Http")),
                IdentifierName("HttpResponseMessage"));

        public static TypeSyntax HttpStatusCode() =>
            QualifiedName(
                QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Net")
                ),
                IdentifierName("HttpStatusCode"));

        public static TypeSyntax IDisposable() =>
            QualifiedName(IdentifierName("System"), IdentifierName("IDisposable"));

        public static TypeSyntax ListT(TypeSyntax itemType) =>
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

        public static TypeSyntax TaskT(TypeSyntax resultType) =>
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

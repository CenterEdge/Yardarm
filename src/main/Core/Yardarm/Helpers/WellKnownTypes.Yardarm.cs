using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberHidesStaticFromOuterClass
    public static partial class WellKnownTypes
    {
        public static partial class Yardarm
        {
            public static NameSyntax Name { get; } = AliasQualifiedName(
                IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                IdentifierName("Yardarm"));

            public static class Client
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    Yardarm.Name,
                    IdentifierName("Client"));

                public static class Internal
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Client.Name,
                        IdentifierName("Internal"));

                    public static class ThrowHelper
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Internal.Name,
                            IdentifierName("ThrowHelper"));
                    }

                }
            }
        }
    }
}

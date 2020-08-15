using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberHidesStaticFromOuterClass
    public static partial class WellKnownTypes
    {
        public static class Yardarm
        {
            public static NameSyntax Name => IdentifierName("Yardarm");

            public static class Client
            {
                public static NameSyntax Name => QualifiedName(
                    Yardarm.Name,
                    IdentifierName("Client"));

                public static class OperationHelpers
                {
                    public static NameSyntax Name => QualifiedName(
                        Client.Name,
                        IdentifierName("OperationHelpers"));

                    public static MemberAccessExpressionSyntax AddQueryParameters => MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        Name,
                        IdentifierName("AddQueryParameters"));
                }
            }
        }
    }
}

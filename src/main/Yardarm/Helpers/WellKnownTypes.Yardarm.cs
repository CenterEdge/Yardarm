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
            public static NameSyntax Name { get; } = AliasQualifiedName(
                IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                IdentifierName("Yardarm"));

            public static class Client
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    Yardarm.Name,
                    IdentifierName("Client"));

                public static class OperationHelpers
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Client.Name,
                        IdentifierName("OperationHelpers"));

                    public static InvocationExpressionSyntax AddQueryParameters(
                        ExpressionSyntax pathExpression, ExpressionSyntax parametersExpression) =>
                        InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                Name,
                                IdentifierName("AddQueryParameters")),
                            ArgumentList(
                                SeparatedList(new[]
                                {
                                    Argument(pathExpression),
                                    Argument(parametersExpression)
                                })));
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class SyntaxHelpers
    {
        public static ExpressionSyntax AwaitConfiguredFalse(ExpressionSyntax awaitable) =>
            AwaitExpression(InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    awaitable,
                    IdentifierName("ConfigureAwait")),
                ArgumentList(SingletonSeparatedList(
                    Argument(LiteralExpression(SyntaxKind.FalseLiteralExpression))))));

        public static bool IsDynamic(TypeSyntax typeSyntax, out bool isNullable)
        {
            isNullable = IsNullable(typeSyntax, out TypeSyntax? innerTypeSyntax);
            if (isNullable)
            {
                typeSyntax = innerTypeSyntax!;
            }

            return typeSyntax is IdentifierNameSyntax nameSyntax && nameSyntax.Identifier.ValueText == "dynamic";
        }

        public static bool IsNullable(TypeSyntax typeSyntax, [MaybeNullWhen(false)] out TypeSyntax innerTypeSyntax)
        {
            if (typeSyntax is NullableTypeSyntax nullableSyntax)
            {
                innerTypeSyntax = nullableSyntax.ElementType;
                return true;
            }
            else
            {
                innerTypeSyntax = null;
                return false;
            }
        }

        public static ExpressionSyntax MemberAccess(params string[] identifierNames) =>
            MemberAccess(IdentifierName(identifierNames[0]), identifierNames.Skip(1));

        public static ExpressionSyntax MemberAccess(ExpressionSyntax left, params string[] identifierNames) =>
            MemberAccess(left, (IEnumerable<string>)identifierNames);

        public static ExpressionSyntax MemberAccess(ExpressionSyntax left, IEnumerable<string> identifierNames) =>
            identifierNames
                .Aggregate(left,
                    (agg, current) =>
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, agg, IdentifierName(current)));

        public static LiteralExpressionSyntax StringLiteral(string value) =>
            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));

        public static ExpressionSyntax ParameterWithNullCheck(string parameterName) =>
            BinaryExpression(SyntaxKind.CoalesceExpression,
                IdentifierName(parameterName),
                ThrowExpression(ObjectCreationExpression(
                    QualifiedName(IdentifierName("System"), IdentifierName("ArgumentNullException")),
                    ArgumentList(SingletonSeparatedList(Argument(
                        StringLiteral(parameterName)))),
                    null)));
    }
}

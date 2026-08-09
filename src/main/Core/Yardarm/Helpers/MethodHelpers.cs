using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Frameworks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class MethodHelpers
    {
        public const string CancellationTokenParameterName = "cancellationToken";

        public static ParameterSyntax DefaultedCancellationTokenParameter() =>
            Parameter(Identifier(CancellationTokenParameterName))
                .WithType(WellKnownTypes.System.Threading.CancellationToken.Name)
                .WithDefault(EqualsValueClause(
                    LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                        Token(SyntaxKind.DefaultKeyword))));

        public static StatementSyntax IfNull(ExpressionSyntax expressionToTest, BlockSyntax trueBlock) =>
            IfStatement(
                IsPatternExpression(
                    expressionToTest,
                    ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                trueBlock);

        public static StatementSyntax IfNotNull(ExpressionSyntax expressionToTest, BlockSyntax trueBlock) =>
            IfStatement(
                IsPatternExpression(
                    expressionToTest,
                    UnaryPattern(Token(SyntaxKind.NotKeyword), ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression)))),
                trueBlock);

        public static StatementSyntax LocalVariableDeclarationWithInitializer(string variableName,
            ExpressionSyntax initializer) =>
            LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .AddVariables(VariableDeclarator(variableName).WithInitializer(EqualsValueClause(initializer))));

        public static StatementSyntax ThrowIfArgumentNull(string parameterName, NuGetFramework? targetFramework = null)
        {
            return ExpressionStatement(InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    WellKnownTypes.System.ArgumentNullException.Name,
                    IdentifierName("ThrowIfNull")),
                ArgumentList(SingletonSeparatedList(Argument(IdentifierName(parameterName))))));
        }
    }
}

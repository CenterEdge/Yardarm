using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class ResponsesBaseTypeGenerator : TypeGeneratorBase
    {
        private const string BaseClassName = "ResponseBase";

        public ResponsesBaseTypeGenerator(GenerationContext context)
            : base(context)
        {
        }

        public override TypeSyntax GetTypeName()
        {
            var ns = Context.NamespaceProvider.GetRootNamespace();

            return QualifiedName(ns, IdentifierName(BaseClassName));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            ClassDeclarationSyntax declaration = ClassDeclaration(BaseClassName)
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.AbstractKeyword))
                .AddMembers(
                    GenerateConstructor(),
                    GenerateProperty());

            yield return declaration;
        }

        private ConstructorDeclarationSyntax GenerateConstructor() =>
            ConstructorDeclaration(BaseClassName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("message")).WithType(HttpResponseMessage()))
                .WithBody(Block(
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName("Message"),
                        SyntaxHelpers.ParameterWithNullCheck("message")))
                    ));

        private PropertyDeclarationSyntax GenerateProperty() =>
            PropertyDeclaration(HttpResponseMessage(), Identifier("Message"))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        private TypeSyntax HttpResponseMessage() =>
            QualifiedName(
                QualifiedName(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("Net")),
                    IdentifierName("Http")),
                IdentifierName("HttpResponseMessage"));
    }
}

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class ResponseBaseInterfaceTypeGenerator : TypeGeneratorBase
    {
        private const string BaseInterfaceName = "IOperationResponse";

        public const string MessageProperty = "Message";
        public const string IsSuccessStatusCodeProperty = "IsSuccessStatusCode";
        public const string StatusCodeProperty = "StatusCode";

        private readonly IRootNamespace _rootNamespace;

        public ResponseBaseInterfaceTypeGenerator(GenerationContext context, IRootNamespace rootNamespace)
            : base(context)
        {
            _rootNamespace = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
        }

        protected override TypeSyntax GetTypeName() =>
            QualifiedName(_rootNamespace.Name, IdentifierName(BaseInterfaceName));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            InterfaceDeclarationSyntax declaration = InterfaceDeclaration(BaseInterfaceName)
                .AddBaseListTypes(
                    SimpleBaseType(WellKnownTypes.System.IDisposable.Name))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    GenerateMessageProperty(),
                    GenerateIsSuccessStatusCodeProperty(),
                    GenerateStatusCodeProperty());

            yield return declaration;
        }

        #region Properties

        private PropertyDeclarationSyntax GenerateMessageProperty() =>
            PropertyDeclaration(WellKnownTypes.System.Net.Http.HttpResponseMessage.Name, Identifier(MessageProperty))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        private PropertyDeclarationSyntax GenerateIsSuccessStatusCodeProperty() =>
            PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), Identifier(IsSuccessStatusCodeProperty))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        private PropertyDeclarationSyntax GenerateStatusCodeProperty() =>
            PropertyDeclaration(WellKnownTypes.System.Net.HttpStatusCode.Name, Identifier(StatusCodeProperty))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        #endregion
    }
}

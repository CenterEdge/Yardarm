using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class ResponsesTypeGenerator : TypeGeneratorBase<OpenApiResponses>
    {
        private readonly ResponsesBaseTypeGenerator _responsesBaseTypeGenerator;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        protected OpenApiResponses Responses => Element.Element;
        protected OpenApiOperation Operation { get; }

        public ResponsesTypeGenerator(LocatedOpenApiElement<OpenApiResponses> element, GenerationContext context,
            ResponsesBaseTypeGenerator responsesBaseTypeGenerator,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
            : base(element, context)
        {
            _responsesBaseTypeGenerator = responsesBaseTypeGenerator ?? throw new ArgumentNullException(nameof(responsesBaseTypeGenerator));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                            throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));

            Operation = (OpenApiOperation)element.Parents[0].Element;
        }

        public override TypeSyntax GetTypeName()
        {
            var ns = Context.NamespaceProvider.GetNamespace(Element);

            return QualifiedName(ns, IdentifierName(GetClassName()));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var className = GetClassName();

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(_responsesBaseTypeGenerator.GetTypeName()))
                .AddMembers(
                    GenerateConstructor(className));

            yield return declaration;
        }

        private ConstructorDeclarationSyntax GenerateConstructor(string className) =>
            ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("message")).WithType(HttpResponseMessage()))
                .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(Argument(IdentifierName("message"))))
                .WithBody(Block());

        private string GetClassName() => Context.NameFormatterSelector.GetFormatter(NameKind.Class).Format(Operation.OperationId + "Response");

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

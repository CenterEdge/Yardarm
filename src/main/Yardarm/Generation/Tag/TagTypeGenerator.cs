using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGenerator : TagTypeGeneratorBase
    {
        private readonly IApiNamespace _apiNamespace;

        public TagTypeGenerator(ILocatedOpenApiElement<OpenApiTag> tagElement, GenerationContext context,
            IApiNamespace apiNamespace, IOperationNameProvider operationNameProvider)
            : base(tagElement, context, operationNameProvider)
        {
            ArgumentNullException.ThrowIfNull(apiNamespace);

            _apiNamespace = apiNamespace;
        }

        protected override YardarmTypeInfo GetTypeInfo() =>
            new YardarmTypeInfo(
            QualifiedName(
                Context.NamespaceProvider.GetNamespace(Element),
                IdentifierName(GetInterfaceName())),
                    NameKind.Interface);

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            yield return GenerateInterface();
        }

        protected virtual MemberDeclarationSyntax GenerateInterface()
        {
            var declaration = InterfaceDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword)),
                Identifier(GetInterfaceName()),
                null,
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(_apiNamespace.IApi))),
                default,
                List<MemberDeclarationSyntax>(
                    GetOperations()
                        .SelectMany(GenerateOperationMethodHeader,
                            (operation, method) => new {operation, method})
                        .Select(p => p.method
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))));

            return declaration;
        }

        private string GetInterfaceName() => Context.NameFormatterSelector.GetFormatter(NameKind.Interface).Format(Tag.Name);
    }
}

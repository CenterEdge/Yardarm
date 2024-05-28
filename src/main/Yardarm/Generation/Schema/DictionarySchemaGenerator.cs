using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public class DictionarySchemaGenerator(
        ILocatedOpenApiElement<OpenApiSchema> schemaElement,
        GenerationContext context,
        ITypeGenerator? parent)
        : TypeGeneratorBase<OpenApiSchema>(schemaElement, context, parent)
    {
        protected override YardarmTypeInfo GetTypeInfo() =>
            new(WellKnownTypes.System.Collections.Generic.DictionaryT.Name(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    Context.TypeGeneratorRegistry.Get(Element.GetAdditionalPropertiesOrDefault()).TypeInfo.Name),
                isGenerated: false);

        public override SyntaxTree? GenerateSyntaxTree() => null;

        public override IEnumerable<MemberDeclarationSyntax> Generate() => [];
    }
}

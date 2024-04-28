using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public sealed class DynamicSchemaGenerator(
        ILocatedOpenApiElement<OpenApiSchema> element,
        GenerationContext context,
        ITypeGenerator? parent)
        : TypeGeneratorBase<OpenApiSchema>(element, context, parent)
    {
        internal static YardarmTypeInfo DynamicObjectTypeInfo { get; }  = new(
            NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))),
            isGenerated: false,
            requiresDynamicSerialization: true);

        protected override YardarmTypeInfo GetTypeInfo() => DynamicObjectTypeInfo;

        public override IEnumerable<MemberDeclarationSyntax> Generate() => [];
    }
}

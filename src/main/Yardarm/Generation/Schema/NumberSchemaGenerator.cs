using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public class NumberSchemaGenerator : TypeGeneratorBase<OpenApiSchema>
    {
        public NumberSchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context,
            ITypeGenerator? parent)
            : base(schemaElement, context, parent)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo() =>
            new YardarmTypeInfo(
                (Element.Element.Type, Element.Element.Format) switch
                {
                    (_, "int32") => PredefinedType(Token(SyntaxKind.IntKeyword)),
                    (_, "integer") => PredefinedType(Token(SyntaxKind.IntKeyword)),
                    (_, "int") => PredefinedType(Token(SyntaxKind.IntKeyword)),
                    (_, "int64") => PredefinedType(Token(SyntaxKind.LongKeyword)),
                    (_, "byte") => PredefinedType(Token(SyntaxKind.ByteKeyword)),
                    ("integer", _) => PredefinedType(Token(SyntaxKind.LongKeyword)),
                    ("number", "decimal") => PredefinedType(Token(SyntaxKind.DecimalKeyword)),
                    ("number", "float") => PredefinedType(Token(SyntaxKind.FloatKeyword)),
                    ("number", _) => PredefinedType(Token(SyntaxKind.DoubleKeyword)),
                    _ => PredefinedType(Token(SyntaxKind.ObjectKeyword))
                },
                NameKind.Struct,
                isGenerated: false);

        public override SyntaxTree? GenerateSyntaxTree() => null;

        public override IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}

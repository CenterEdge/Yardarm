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
    public class StringSchemaGenerator : TypeGeneratorBase<OpenApiSchema>
    {
        public StringSchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context,
            ITypeGenerator? parent)
            : base(schemaElement, context, parent)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo() =>
            new YardarmTypeInfo(
                Element.Element.Format switch
                {
                    "date" or "full-date" => QualifiedName(IdentifierName("System"), IdentifierName("DateTime")),
                    "partial-time" => QualifiedName(IdentifierName("System"), IdentifierName("TimeSpan")),
                    "date-time" => QualifiedName(IdentifierName("System"), IdentifierName("DateTimeOffset")),
                    "uuid" => QualifiedName(IdentifierName("System"), IdentifierName("Guid")),
                    "uri" => QualifiedName(IdentifierName("System"), IdentifierName("Uri")),
                    "byte" => ArrayType(PredefinedType(Token(SyntaxKind.ByteKeyword)),
                        SingletonList(ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))),
                    "binary" => QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("IO")),
                        IdentifierName("Stream")),
                    _ => PredefinedType(Token(SyntaxKind.StringKeyword))
                },
                isGenerated: false);

        public override SyntaxTree? GenerateSyntaxTree() => null;

        public override IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}

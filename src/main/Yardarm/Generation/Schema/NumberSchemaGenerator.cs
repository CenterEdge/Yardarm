using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public class NumberSchemaGenerator(
        ILocatedOpenApiElement<OpenApiSchema> schemaElement,
        GenerationContext context,
        ITypeGenerator? parent)
        : TypeGeneratorBase<OpenApiSchema>(schemaElement, context, parent)
    {
        private static YardarmTypeInfo? s_integer;
        private static YardarmTypeInfo Integer => s_integer ??= new YardarmTypeInfo(
            PredefinedType(Token(SyntaxKind.IntKeyword)), NameKind.Struct, isGenerated: false);

        private static YardarmTypeInfo? s_long;
        private static YardarmTypeInfo Long => s_long ??= new YardarmTypeInfo(
            PredefinedType(Token(SyntaxKind.LongKeyword)), NameKind.Struct, isGenerated: false);

        private static YardarmTypeInfo? s_byte;
        private static YardarmTypeInfo Byte => s_byte ??= new YardarmTypeInfo(
            PredefinedType(Token(SyntaxKind.ByteKeyword)), NameKind.Struct, isGenerated: false);

        private static YardarmTypeInfo? s_decimal;
        private static YardarmTypeInfo Decimal => s_decimal ??= new YardarmTypeInfo(
            PredefinedType(Token(SyntaxKind.DecimalKeyword)), NameKind.Struct, isGenerated: false);

        private static YardarmTypeInfo? s_float;
        private static YardarmTypeInfo Float => s_float ??= new YardarmTypeInfo(
            PredefinedType(Token(SyntaxKind.FloatKeyword)), NameKind.Struct, isGenerated: false);

        private static YardarmTypeInfo? s_double;
        private static YardarmTypeInfo Double => s_double ??= new YardarmTypeInfo(
            PredefinedType(Token(SyntaxKind.DoubleKeyword)), NameKind.Struct, isGenerated: false);

        protected override YardarmTypeInfo GetTypeInfo() =>
            (Element.Element.Type, Element.Element.Format) switch
            {
                (_, "int32") => Integer,
                (_, "integer") => Integer,
                (_, "int") => Integer,
                (_, "int64") => Long,
                (_, "byte") => Byte,
                ("integer", _) => Long,
                ("number", "decimal") => Decimal,
                ("number", "float") => Float,
                ("number", _) => Double,
                _ => DynamicSchemaGenerator.DynamicObjectTypeInfo
            };

        public override SyntaxTree? GenerateSyntaxTree() => null;

        public override IEnumerable<MemberDeclarationSyntax> Generate() => [];
    }
}

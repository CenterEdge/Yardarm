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
    public class StringSchemaGenerator(
        ILocatedOpenApiElement<OpenApiSchema> schemaElement,
        GenerationContext context,
        ITypeGenerator? parent)
        : TypeGeneratorBase<OpenApiSchema>(schemaElement, context, parent)
    {
        private static YardarmTypeInfo? s_string;
        private static YardarmTypeInfo String => s_string ??= new YardarmTypeInfo(
            PredefinedType(Token(SyntaxKind.StringKeyword)), isGenerated: false);

        private static YardarmTypeInfo? s_dateTime;
        private static YardarmTypeInfo DateTime => s_dateTime ??= new YardarmTypeInfo(
            QualifiedName(IdentifierName("System"), IdentifierName("DateTime")), isGenerated: false);

        private static YardarmTypeInfo? s_dateTimeOffset;
        private static YardarmTypeInfo DateTimeOffset => s_dateTimeOffset ??= new YardarmTypeInfo(
            QualifiedName(IdentifierName("System"), IdentifierName("DateTimeOffset")), isGenerated: false);

        private static YardarmTypeInfo? s_timeSpan;
        private static YardarmTypeInfo TimeSpan => s_timeSpan ??= new YardarmTypeInfo(
            QualifiedName(IdentifierName("System"), IdentifierName("TimeSpan")), isGenerated: false);

        private static YardarmTypeInfo? s_guid;
        private static YardarmTypeInfo Guid => s_guid ??= new YardarmTypeInfo(
            QualifiedName(IdentifierName("System"), IdentifierName("Guid")), isGenerated: false);

        private static YardarmTypeInfo? s_uri;
        private static YardarmTypeInfo Uri => s_uri ??= new YardarmTypeInfo(
            QualifiedName(IdentifierName("System"), IdentifierName("Uri")), isGenerated: false);

        private static YardarmTypeInfo? s_byteArray;
        private static YardarmTypeInfo ByteArray => s_byteArray ??= new YardarmTypeInfo(
            ArrayType(PredefinedType(Token(SyntaxKind.ByteKeyword)),
                SingletonList(ArrayRankSpecifier(
                    SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))),
            isGenerated: false);

        private static YardarmTypeInfo? s_binary;
        private static YardarmTypeInfo Binary => s_binary ??= new YardarmTypeInfo(
            WellKnownTypes.System.IO.Stream.Name, isGenerated: false);

        protected override YardarmTypeInfo GetTypeInfo() =>
            Element.Element.Format switch
            {
                "date" or "full-date" => DateTime,
                "partial-time" or "date-span" => TimeSpan,
                "date-time" => DateTimeOffset,
                "uuid" => Guid,
                "uri" => Uri,
                "byte" => ByteArray,
                "binary" => Binary,
                _ => String
            };

        public override SyntaxTree? GenerateSyntaxTree() => null;

        public override IEnumerable<MemberDeclarationSyntax> Generate() => [];
    }
}

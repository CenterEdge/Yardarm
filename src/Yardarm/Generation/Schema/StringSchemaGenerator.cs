using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public class StringSchemaGenerator : ITypeGenerator
    {
        private readonly LocatedOpenApiElement<OpenApiSchema> _schemaElement;

        public StringSchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement)
        {
            _schemaElement = schemaElement ?? throw new ArgumentNullException(nameof(schemaElement));
        }

        public void Preprocess()
        {
        }

        public TypeSyntax GetTypeName() =>
            _schemaElement.Element.Format switch
            {
                "date"=> QualifiedName(IdentifierName("System"), IdentifierName("DateTime")),
                "date-time" => QualifiedName(IdentifierName("System"), IdentifierName("DateTimeOffset")),
                "uuid" => QualifiedName(IdentifierName("System"), IdentifierName("Guid")),
                "uri" => QualifiedName(IdentifierName("System"), IdentifierName("Uri")),
                "byte" => ArrayType(PredefinedType(Token(SyntaxKind.ByteKeyword)),
                    SingletonList(ArrayRankSpecifier(
                        SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))),
                "binary" => QualifiedName(QualifiedName(IdentifierName("System"), IdentifierName("IO")), IdentifierName("Stream")),
                _ => PredefinedType(Token(SyntaxKind.StringKeyword))
            };

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}

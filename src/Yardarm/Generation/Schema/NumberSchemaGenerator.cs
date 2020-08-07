using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class NumberSchemaGenerator : ITypeGenerator
    {
        private readonly LocatedOpenApiElement<OpenApiSchema> _schemaElement;

        public NumberSchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement)
        {
            _schemaElement = schemaElement ?? throw new ArgumentNullException(nameof(schemaElement));
        }

        public void Preprocess()
        {
        }

        public TypeSyntax GetTypeName() =>
            (_schemaElement.Element.Type, _schemaElement.Element.Format) switch
            {
                (_, "int32") => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                (_, "integer") => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                (_, "int") => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                (_, "int64") => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
                (_, "byte") => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)),
                ("integer", _) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
                ("number", "decimal") => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DecimalKeyword)),
                ("number", "float") => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword)),
                ("number", _) => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)),
                _ => SyntaxFactory.IdentifierName("dynamic")
            };

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}

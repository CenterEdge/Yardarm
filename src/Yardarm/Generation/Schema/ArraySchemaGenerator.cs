using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;

namespace Yardarm.Generation.Schema
{
    public class ArraySchemaGenerator : ISchemaGenerator
    {
        private readonly ISchemaGeneratorFactory _schemaGeneratorFactory;

        public ArraySchemaGenerator(ISchemaGeneratorFactory schemaGeneratorFactory)
        {
            _schemaGeneratorFactory = schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
        }

        public TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiSchema> schemaElement)
        {
            // Treat the items as having the same parent as the array, otherwise we get into an infinite name loop since
            // we're not making a custom class for the list.
            var itemElement = schemaElement.CreateChild(schemaElement.Element.Items, schemaElement.Key);

            TypeSyntax itemTypeName = _schemaGeneratorFactory.Get(itemElement).GetTypeName(itemElement);

            return SyntaxHelpers.ListT(itemTypeName);
        }

        public SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiSchema> element) => null;

        public IEnumerable<MemberDeclarationSyntax> Generate(LocatedOpenApiElement<OpenApiSchema> element) =>
            Enumerable.Empty<MemberDeclarationSyntax>();
    }
}

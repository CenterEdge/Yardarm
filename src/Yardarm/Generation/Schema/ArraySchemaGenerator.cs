using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class ArraySchemaGenerator : ITypeGenerator
    {
        protected LocatedOpenApiElement<OpenApiSchema> SchemaElement { get; }
        protected GenerationContext Context { get; }

        protected OpenApiSchema Schema => SchemaElement.Element;

        public ArraySchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
        {
            SchemaElement = schemaElement ?? throw new ArgumentNullException(nameof(schemaElement));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Preprocess()
        {
        }

        public TypeSyntax GetTypeName()
        {
            // Treat the items as having the same parent as the array, otherwise we get into an infinite name loop since
            // we're not making a custom class for the list.
            var itemElement = SchemaElement.CreateChild(Schema.Items, SchemaElement.Key);

            TypeSyntax itemTypeName = Context.SchemaGeneratorRegistry.Get(itemElement).GetTypeName();

            return WellKnownTypes.ListT(itemTypeName);
        }

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() => Enumerable.Empty<MemberDeclarationSyntax>();
    }
}

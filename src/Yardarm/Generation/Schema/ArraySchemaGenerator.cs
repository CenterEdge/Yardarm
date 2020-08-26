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
        private TypeSyntax? _nameCache;

        public TypeSyntax TypeName => _nameCache ??= GetTypeName();

        protected ILocatedOpenApiElement<OpenApiSchema> SchemaElement { get; }
        protected GenerationContext Context { get; }

        protected OpenApiSchema Schema => SchemaElement.Element;

        public ArraySchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
        {
            SchemaElement = schemaElement ?? throw new ArgumentNullException(nameof(schemaElement));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected TypeSyntax GetTypeName()
        {
            TypeSyntax itemTypeName = Context.SchemaGeneratorRegistry.Get(GetItemSchema()).TypeName;

            return WellKnownTypes.System.Collections.Generic.ListT.Name(itemTypeName);
        }

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate()
        {
            ILocatedOpenApiElement<OpenApiSchema> itemSchema = GetItemSchema();

            return itemSchema.Element.Reference is null
                ? Context.SchemaGeneratorRegistry.Get(itemSchema).Generate()
                : Enumerable.Empty<MemberDeclarationSyntax>();
        }

        private ILocatedOpenApiElement<OpenApiSchema> GetItemSchema() =>
            // Treat the items as having the same parent as the array, otherwise we get into an infinite name loop since
            // we're not making a custom class for the list.
            SchemaElement.Parent != null
                ? SchemaElement.Parent.CreateChild(Schema.Items, SchemaElement.Key + "-Item")
                : Schema.Items.CreateRoot(SchemaElement.Key + "-Item");
    }
}

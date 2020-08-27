using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class ArraySchemaGenerator : TypeGeneratorBase<OpenApiSchema>
    {
        protected OpenApiSchema Schema => Element.Element;

        public ArraySchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            TypeSyntax itemTypeName = Context.TypeGeneratorRegistry.Get(GetItemSchema()).TypeInfo.Name;

            return new YardarmTypeInfo(
                WellKnownTypes.System.Collections.Generic.ListT.Name(itemTypeName),
                isGenerated: false);
        }

        public override SyntaxTree? GenerateSyntaxTree() => null;

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            ILocatedOpenApiElement<OpenApiSchema> itemSchema = GetItemSchema();

            return itemSchema.Element.Reference is null
                ? Context.TypeGeneratorRegistry.Get(itemSchema).Generate()
                : Enumerable.Empty<MemberDeclarationSyntax>();
        }

        private ILocatedOpenApiElement<OpenApiSchema> GetItemSchema() =>
            // Treat the items as having the same parent as the array, otherwise we get into an infinite name loop since
            // we're not making a custom class for the list.
            Element.Parent != null
                ? Element.Parent.CreateChild(Schema.Items, Element.Key + "-Item")
                : Schema.Items.CreateRoot(Element.Key + "-Item");
    }
}

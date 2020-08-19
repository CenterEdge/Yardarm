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
            // Treat the items as having the same parent as the array, otherwise we get into an infinite name loop since
            // we're not making a custom class for the list.
            var itemElement = SchemaElement.CreateChild(Schema.Items, SchemaElement.Key);

            TypeSyntax itemTypeName = Context.SchemaGeneratorRegistry.Get(itemElement).TypeName;

            return WellKnownTypes.System.Collections.Generic.ListT.Name(itemTypeName);
        }

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() => Enumerable.Empty<MemberDeclarationSyntax>();
    }
}

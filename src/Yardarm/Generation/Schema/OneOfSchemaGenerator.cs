using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment.Schema;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    /// <summary>
    /// Generates the generic OneOf&gt;...&lt; types for various discriminated unions.
    /// </summary>
    internal class OneOfSchemaGenerator : SchemaGeneratorBase
    {
        protected override NameKind NameKind => NameKind.Interface;

        public OneOfSchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        public override TypeSyntax GetTypeName() =>
            !HasDiscriminator
                ? SyntaxFactory.IdentifierName("dynamic")
                : base.GetTypeName();

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            if (!HasDiscriminator)
            {
                // We have no discriminator, so we're just going to implement using dynamic expando objects
                // So we shouldn't implement a specific class
                yield break;
            }

            var interfaceNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            // Register the referenced schema to implement this interface
            var baseTypeRegistry = Context.GenerationServices.GetRequiredService<ISchemaBaseTypeRegistry>();
            foreach (var referencedSchema in Schema.OneOf
                .Where(p => p.Reference != null)
                .Select(p => ((OpenApiSchema) Context.Document.ResolveReference(p.Reference)).CreateRoot(p.Reference.Id)))
            {
                baseTypeRegistry.AddBaseType(referencedSchema, SyntaxFactory.SimpleBaseType(interfaceNameAndNamespace));
            }

            SimpleNameSyntax interfaceName = interfaceNameAndNamespace.Right;

            yield return SyntaxFactory.InterfaceDeclaration(interfaceName.ToString())
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }

        private bool HasDiscriminator => Schema.Discriminator?.PropertyName != null;
    }
}

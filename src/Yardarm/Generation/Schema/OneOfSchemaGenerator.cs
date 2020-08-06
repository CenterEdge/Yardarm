﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Features;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    /// <summary>
    /// Generates the generic OneOf&gt;...&lt; types for various discriminated unions.
    /// </summary>
    internal class OneOfSchemaGenerator : SchemaGeneratorBase
    {
        protected override NameKind NameKind => NameKind.Class;

        protected override string NamePrefix => "I"; // We're making an interface

        public OneOfSchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        public override TypeSyntax GetTypeName() =>
            !HasDiscriminator
                ? SyntaxFactory.IdentifierName("dynamic")
                : base.GetTypeName();

        public override void Preprocess()
        {
            if (!HasDiscriminator)
            {
                // We have no discriminator, so we're just going to implement using dynamic expando objects
                // So we shouldn't implement a specific class
                return;
            }

            TypeSyntax interfaceNameAndNamespace = GetTypeName();

            // Register the referenced schema to implement this interface
            ISchemaBaseTypeFeature baseTypeFeature = Context.Features.GetOrAdd<ISchemaBaseTypeFeature, SchemaBaseTypeFeature>();
            foreach (var referencedSchema in Schema.OneOf
                .Where(p => p.Reference != null)
                .Select(p => ((OpenApiSchema) Context.Document.ResolveReference(p.Reference)).CreateRoot(p.Reference.Id)))
            {
                baseTypeFeature.AddBaseType(referencedSchema, SyntaxFactory.SimpleBaseType(interfaceNameAndNamespace));
            }
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            if (!HasDiscriminator)
            {
                // We have no discriminator, so we're just going to implement using dynamic expando objects
                // So we shouldn't implement a specific class
                yield break;
            }

            var interfaceNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            SimpleNameSyntax interfaceName = interfaceNameAndNamespace.Right;

            var interfaceDeclaration = SyntaxFactory.InterfaceDeclaration(interfaceName.ToString())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            yield return interfaceDeclaration.Enrich(Context.Enrichers.InterfaceEnrichers, SchemaElement);
        }

        private bool HasDiscriminator => Schema.Discriminator?.PropertyName != null;
    }
}
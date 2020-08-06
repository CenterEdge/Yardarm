using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    internal class AllOfSchemaGenerator : ObjectSchemaGenerator
    {
        public AllOfSchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            foreach (MemberDeclarationSyntax child in base.Generate())
            {
                if (child is ClassDeclarationSyntax classDeclaration)
                {
                    bool addedInheritance = false;
                    foreach (var section in Schema.AllOf)
                    {
                        if (!addedInheritance && section.Reference != null)
                        {
                            // We can inherit from the reference, but we need to load it from the reference to get the right type name

                            LocatedOpenApiElement<OpenApiSchema> referencedSchema =
                                ((OpenApiSchema)Context.Document.ResolveReference(section.Reference)).CreateRoot(section.Reference.Id);

                            TypeSyntax typeName = Context.TypeNameGenerator.GetName(referencedSchema);

                            BaseListSyntax baseList = classDeclaration.BaseList != null
                                ? classDeclaration.BaseList.WithTypes(SyntaxFactory.SeparatedList(
                                    new[] {SyntaxFactory.SimpleBaseType(typeName)}.Concat(classDeclaration.BaseList.Types)))
                                : SyntaxFactory.BaseList(
                                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(SyntaxFactory.SimpleBaseType(typeName)));

                            classDeclaration = classDeclaration
                                .WithBaseList(baseList);

                            addedInheritance = true;
                        }
                        else
                        {
                            classDeclaration = AddProperties(classDeclaration, SchemaElement, section.Properties);
                        }
                    }

                    yield return classDeclaration;
                }
                else
                {
                    yield return child;
                }
            }

        }
    }
}

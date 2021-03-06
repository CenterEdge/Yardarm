﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    internal class AllOfSchemaGenerator : ObjectSchemaGenerator
    {
        public AllOfSchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context,
            ITypeGenerator? parent)
            : base(schemaElement, context, parent)
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

                            ILocatedOpenApiElement<OpenApiSchema> referencedSchema =
                                ((OpenApiSchema)Context.Document.ResolveReference(section.Reference)).CreateRoot(section.Reference.Id);

                            TypeSyntax typeName = Context.TypeGeneratorRegistry.Get(referencedSchema).TypeInfo.Name;

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
                            classDeclaration = AddProperties(classDeclaration, section.Properties
                                .Select(p => Element.CreateChild(p.Value, p.Key)));
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

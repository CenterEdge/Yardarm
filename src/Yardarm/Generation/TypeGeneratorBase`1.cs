﻿using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public abstract class TypeGeneratorBase<T> : TypeGeneratorBase
        where T : IOpenApiElement
    {
        public ILocatedOpenApiElement<T> Element { get; }

        protected TypeGeneratorBase(ILocatedOpenApiElement<T> element, GenerationContext context, ITypeGenerator? parent)
            : base(context, parent)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public override CompilationUnitSyntax GenerateCompilationUnit(MemberDeclarationSyntax[] members) =>
            base.GenerateCompilationUnit(members)
                // Annotate the overall compilation unit with the element so it may be enriched with additional classes, etc
                .AddElementAnnotation(Element, Context.ElementRegistry);
    }
}

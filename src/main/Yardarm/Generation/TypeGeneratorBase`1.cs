using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Helpers;
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

        /// <inheritdoc />
        protected override string? GetSourceFilePath()
        {
            string? elementPath = Element.ToString();
            if (string.IsNullOrEmpty(elementPath))
            {
                return null;
            }

            if (elementPath[0] == '/')
            {
                elementPath = $"{elementPath[1..]}.cs";
            }
            else
            {
                elementPath = $"{elementPath}.cs";
            }

            return Path.Combine(Context.Settings.BasePath, PathHelpers.NormalizePath(elementPath));
        }
    }
}

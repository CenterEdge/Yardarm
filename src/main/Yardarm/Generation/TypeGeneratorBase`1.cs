using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            ArgumentNullException.ThrowIfNull(element);

            Element = element;
        }

        public override CompilationUnitSyntax GenerateCompilationUnit(MemberDeclarationSyntax[] members) =>
            base.GenerateCompilationUnit(members)
                // Annotate the overall compilation unit with the element so it may be enriched with additional classes, etc
                .AddElementAnnotation(Element, Context.ElementRegistry);

        /// <inheritdoc />
        protected override string? GetSourceFilePath()
        {
            StringBuilder builder = StringBuilderCache.Acquire();
            try
            {
                builder.Append(Context.Settings.BasePath);
                if (Context.Settings.BasePath.EndsWith("/"))
                {
                    builder.Length -= 1;
                }

                if (TypeInfo.Name is QualifiedNameSyntax qualifiedName)
                {
                    var stack = new Stack<SimpleNameSyntax>(4);
                    QualifiedNameSyntax current = qualifiedName;
                    while (true)
                    {
                        stack.Push(current.Right);

                        if (current.Left is QualifiedNameSyntax leftQualifiedName)
                        {
                            current = leftQualifiedName;
                        }
                        else
                        {
                            if (current.Left is SimpleNameSyntax simpleName)
                            {
                                stack.Push(simpleName);
                            }
                            else if (current.Left is AliasQualifiedNameSyntax aliasedName)
                            {
                                stack.Push(aliasedName.Name);
                            }

                            break;
                        }
                    }

                    while (stack.Count > 0)
                    {
                        builder.Append('/');
                        builder.Append(stack.Pop().Identifier.ValueText);
                    }
                }
                else
                {
                    // Fallback for corner cases

                    string? elementPath = Element.ToString();
                    if (string.IsNullOrEmpty(elementPath))
                    {
                        return null;
                    }

                    if (elementPath[0] != '/')
                    {
                        elementPath = $"/{elementPath}";
                    }

                    builder.Append(PathHelpers.NormalizePath(elementPath));
                }

                builder.Append(".cs");
                return builder.ToString();
            }
            finally
            {
                StringBuilderCache.Release(builder);
            }
        }
    }
}

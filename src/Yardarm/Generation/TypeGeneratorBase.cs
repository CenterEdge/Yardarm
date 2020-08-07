using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public abstract class TypeGeneratorBase<T> : ITypeGenerator
        where T : IOpenApiSerializable
    {
        protected LocatedOpenApiElement<T> Element { get; }
        protected GenerationContext Context { get; }

        protected TypeGeneratorBase(LocatedOpenApiElement<T> element, GenerationContext context)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual void Preprocess()
        {
        }

        public abstract TypeSyntax GetTypeName();

        public virtual SyntaxTree? GenerateSyntaxTree()
        {
            var members = Generate().ToArray();
            if (members.Length == 0)
            {
                return null;
            }

            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            NameSyntax ns = classNameAndNamespace.Left;

            return CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(members))
                .NormalizeWhitespace());
        }

        public abstract IEnumerable<MemberDeclarationSyntax> Generate();
    }
}

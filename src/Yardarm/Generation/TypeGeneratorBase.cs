using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Generation
{
    public abstract class TypeGeneratorBase : ITypeGenerator
    {
        protected GenerationContext Context { get; }

        protected TypeGeneratorBase(GenerationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
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

            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(members));

            return CSharpSyntaxTree.Create(compilationUnit);
        }

        public abstract IEnumerable<MemberDeclarationSyntax> Generate();
    }
}

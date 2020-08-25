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
        private TypeSyntax? _nameCache;

        public TypeSyntax TypeName => _nameCache ??= GetTypeName();

        protected GenerationContext Context { get; }

        protected TypeGeneratorBase(GenerationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected abstract TypeSyntax GetTypeName();

        public virtual SyntaxTree? GenerateSyntaxTree()
        {
            var members = Generate().ToArray();
            if (members.Length == 0)
            {
                return null;
            }

            var compilationUnit = GenerateCompilationUnit(members);

            return CSharpSyntaxTree.Create(compilationUnit);
        }

        public virtual CompilationUnitSyntax GenerateCompilationUnit(MemberDeclarationSyntax[] members)
        {
            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            NameSyntax ns = classNameAndNamespace.Left;

            return SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(members));
        }

        public abstract IEnumerable<MemberDeclarationSyntax> Generate();
    }
}

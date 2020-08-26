using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;

namespace Yardarm.Generation
{
    public abstract class TypeGeneratorBase : ITypeGenerator
    {
        private YardarmTypeInfo? _typeInfoCache;

        public YardarmTypeInfo TypeInfo => _typeInfoCache ??= GetTypeInfo();

        protected GenerationContext Context { get; }

        protected TypeGeneratorBase(GenerationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected abstract YardarmTypeInfo GetTypeInfo();

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
            var classNameAndNamespace = (QualifiedNameSyntax)TypeInfo.Name;

            NameSyntax ns = classNameAndNamespace.Left;

            return SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(members));
        }

        public abstract IEnumerable<MemberDeclarationSyntax> Generate();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation
{
    public abstract class TypeGeneratorBase : ITypeGenerator
    {
        private YardarmTypeInfo? _typeInfoCache;

        public YardarmTypeInfo TypeInfo => _typeInfoCache ??= GetTypeInfo();

        public ITypeGenerator? Parent { get; }

        protected GenerationContext Context { get; }

        protected TypeGeneratorBase(GenerationContext context, ITypeGenerator? parent)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Parent = parent;
        }

        protected abstract YardarmTypeInfo GetTypeInfo();

        public virtual QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
            where TChild : IOpenApiElement => null;

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

        /// <summary>
        /// Gets the namespace to use when generated a full syntax tree.
        /// By default, this is the left part of the type name from <see cref="TypeInfo"/>.
        /// </summary>
        protected virtual NameSyntax GetNamespace() =>
            ((QualifiedNameSyntax)TypeInfo.Name).Left;

        public virtual CompilationUnitSyntax GenerateCompilationUnit(MemberDeclarationSyntax[] members)
        {
            NameSyntax ns = GetNamespace();

            return SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(members));
        }

        public abstract IEnumerable<MemberDeclarationSyntax> Generate();
    }
}

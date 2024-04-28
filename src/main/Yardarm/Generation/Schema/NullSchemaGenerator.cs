using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    [Obsolete("Use DynamicSchemaGenerator instead.")]
    public class NullSchemaGenerator : ITypeGenerator
    {
        public static NullSchemaGenerator Instance { get; } = new();

        public ITypeGenerator? Parent => null;

        private NullSchemaGenerator()
        {
        }

        public YardarmTypeInfo TypeInfo => DynamicSchemaGenerator.DynamicObjectTypeInfo;

        public SyntaxTree? GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() => [];

        public QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
            where TChild : IOpenApiElement =>
            null;
    }
}

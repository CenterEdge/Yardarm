using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation;

public abstract class TypeGeneratorBase : ITypeGenerator
{
    private YardarmTypeInfo? _typeInfoCache;

    /// <inheritdoc />
    public YardarmTypeInfo TypeInfo => _typeInfoCache ??= CreateTypeInfo();

    public ITypeGenerator? Parent { get; }

    protected GenerationContext Context { get; }

    protected TypeGeneratorBase(GenerationContext context, ITypeGenerator? parent)
    {
        ArgumentNullException.ThrowIfNull(context);

        Context = context;
        Parent = parent;
    }

    /// <inheritdoc/>
    public abstract QualifiedNameSyntax? GetTypeName();

    /// <summary>
    /// Creates the information about the type referenced by this type generator.
    /// </summary>
    /// <returns>The information about the type.</returns>
    /// <remarks>
    /// This information is used to reference the type in other generated code. The <see cref="YardarmTypeInfo.Name"/>
    /// will typically be the name returned by <see cref="GetTypeName"/>.
    /// </remarks>
    protected virtual YardarmTypeInfo CreateTypeInfo()
    {
        var typeName = GetTypeName();
        if (typeName is null)
        {
            ThrowHelpers.ThrowInvalidOperationException(
                $"Unable to generate default type info, no name was returned by GetTypeName.");
        }

        return new(typeName);
    }

    public virtual QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
        where TChild : IOpenApiElement => null;

    public virtual SyntaxTree? GenerateSyntaxTree()
    {
        var members = Generate().ToList();
        if (members.Count == 0)
        {
            return null;
        }

        CompilationUnitSyntax compilationUnit = GenerateCompilationUnit(members);

        return CSharpSyntaxTree.Create(compilationUnit,
            options: Context.ParseOptions,
            path: GetSourceFilePath(),
            encoding: Encoding.UTF8);
    }

    /// <summary>
    /// For types that are not nested, returns the unique file path which should
    /// be associated with the <see cref="SyntaxTree"/>.
    /// </summary>
    /// <returns></returns>
    protected abstract string? GetSourceFilePath();

    /// <summary>
    /// Gets the namespace to use when generated a full syntax tree.
    /// By default, this is the left part of the type name from <see cref="GetTypeName"/>.
    /// </summary>
    protected NameSyntax? GetNamespace() => GetTypeName()?.Left;

    public virtual CompilationUnitSyntax GenerateCompilationUnit(IEnumerable<MemberDeclarationSyntax> members)
    {
        NameSyntax? ns = GetNamespace();
        if (ns is null)
        {
            ThrowHelpers.ThrowInvalidOperationException(
                "Unable to generate compilation unit, no namespace was returned by GetNamespace.");
        }

        return CompilationUnit(
            externs: default,
            usings: default,
            attributeLists: default,
            members: SingletonList<MemberDeclarationSyntax>(
                FileScopedNamespaceDeclaration(
                    attributeLists: default,
                    modifiers: default,
                    namespaceKeyword: Token(SyntaxKind.NamespaceKeyword),
                    name: ns,
                    semicolonToken: Token(SyntaxKind.SemicolonToken),
                    externs: default,
                    usings: default,
                    members: List(members))));
    }

    public abstract IEnumerable<MemberDeclarationSyntax> Generate();
}

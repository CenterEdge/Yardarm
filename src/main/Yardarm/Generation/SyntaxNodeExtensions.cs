using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Generation;

public static class SyntaxNodeExtensions
{
    private const string ResourceNameKey = "ResourceName";
    private const string IncludedFileNameKey = "IncludedFileName";
    private const string SpecialMemberKey = "SpecialMember";

    /// <param name="node">Compilation unit.</param>
    extension(CompilationUnitSyntax node)
    {
        /// <summary>
        /// Annotates a <see cref="CompilationUnitSyntax"/> as derived from a specific resource file name.
        /// </summary>
        /// <param name="resourceName">Resource name which was the source of the compilation unit.</param>
        /// <returns></returns>
        public CompilationUnitSyntax AddResourceNameAnnotation(string resourceName) =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(ResourceNameKey, resourceName));

        /// <summary>
        /// Gets the resource file name that was the source of a <see cref="CompilationUnitSyntax"/>, if any.
        /// </summary>
        /// <returns>The resource name or null.</returns>
        public string? GetResourceNameAnnotation() =>
            node.GetAnnotations(ResourceNameKey).FirstOrDefault()?.Data;

        /// <summary>
        /// Annotates a <see cref="CompilationUnitSyntax"/> as derived from a specific included file name.
        /// </summary>
        /// <param name="resourceName">File name which was the source of the compilation unit.</param>
        /// <returns>Modified compilation unit.</returns>
        public CompilationUnitSyntax AddIncludedFileNameAnnotation(string resourceName) =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(IncludedFileNameKey, resourceName));

        /// <summary>
        /// Gets the included file name that was the source of a <see cref="CompilationUnitSyntax"/>, if any.
        /// </summary>
        /// <returns>The file name or null.</returns>
        public string? GetIncludedFileNameAnnotation() =>
            node.GetAnnotations(IncludedFileNameKey).FirstOrDefault()?.Data;
    }

    /// <param name="node">Syntax node to check.</param>
    extension<T>(T node)
        where T : SyntaxNode
    {
        /// <summary>
        /// Annotates a <see cref="SyntaxNode"/> as a special member.
        /// </summary>
        /// <typeparam name="T">Type of syntax node.</typeparam>
        /// <param name="specialMember">The special member name.</param>
        /// <returns>The modified SyntaxNode.</returns>
        public T AddSpecialMemberAnnotation(string specialMember) =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(SpecialMemberKey, specialMember));

        /// <summary>
        /// Gets the special member annotation on a <see cref="SyntaxNode"/>, if any.
        /// </summary>
        /// <typeparam name="T">Type of syntax node.</typeparam>
        /// <returns>The special member name or null.</returns>
        public string? GetSpecialMemberAnnotation() =>
            node.GetAnnotations(SpecialMemberKey).FirstOrDefault()?.Data;

        /// <summary>
        /// Gets the resource file name that was the source of a <see cref="CompilationUnitSyntax"/>, if any.
        /// </summary>
        /// <param name="specialMember">Special member to find.</param>
        /// <returns>All special members of the given type.</returns>
        public IEnumerable<SyntaxNode> GetSpecialMembers(string specialMember) =>
            node.GetAnnotatedNodes(SpecialMemberKey)
                .Where(p => p.GetSpecialMemberAnnotation() == specialMember);
    }
}

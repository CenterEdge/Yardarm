using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Generation
{
    public static class SyntaxNodeExtensions
    {
        private const string ResourceNameKey = "ResourceName";
        private const string SpecialMemberKey = "SpecialMember";

        /// <summary>
        /// Annotates a <see cref="CompilationUnitSyntax"/> as derived from a specific resource file name.
        /// </summary>
        /// <param name="node">Compilation unit to annotate.</param>
        /// <param name="resourceName">Resource name which was the source of the compilation unit.</param>
        /// <returns></returns>
        public static CompilationUnitSyntax AddResourceNameAnnotation(this CompilationUnitSyntax node,
            string resourceName) =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(ResourceNameKey, resourceName));

        /// <summary>
        /// Gets the resource file name that was the source of a <see cref="CompilationUnitSyntax"/>, if any.
        /// </summary>
        /// <param name="node">Compilation unit to check.</param>
        /// <returns>The resource name or null.</returns>
        public static string? GetResourceNameAnnotation(this CompilationUnitSyntax node) =>
            node.GetAnnotations(ResourceNameKey).FirstOrDefault()?.Data;

        /// <summary>
        /// Annotates a <see cref="SyntaxNode"/> as a special member.
        /// </summary>
        /// <typeparam name="T">Type of syntax node.</typeparam>
        /// <param name="node">Syntax node to annotate.</param>
        /// <param name="specialMember">The special member name.</param>
        /// <returns>The modified SyntaxNode.</returns>
        public static T AddSpecialMemberAnnotation<T>(this T node,
            string specialMember)
            where T : SyntaxNode =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(SpecialMemberKey, specialMember));

        /// <summary>
        /// Gets the special member annotation on a <see cref="SyntaxNode"/>, if any.
        /// </summary>
        /// <typeparam name="T">Type of syntax node.</typeparam>
        /// <param name="node">Syntax node to check.</param>
        /// <returns>The special member name or null.</returns>
        public static string? GetSpecialMemberAnnotation<T>(this T node)
            where T : SyntaxNode =>
            node.GetAnnotations(SpecialMemberKey).FirstOrDefault()?.Data;

        /// <summary>
        /// Gets the resource file name that was the source of a <see cref="CompilationUnitSyntax"/>, if any.
        /// </summary>
        /// <param name="parentNode">Syntax node to check.</param>
        /// <param name="specialMember">Special member to find.</param>
        /// <returns>All special members of the given type.</returns>
        public static IEnumerable<SyntaxNode> GetSpecialMembers(this SyntaxNode parentNode, string specialMember) =>
            parentNode.GetAnnotatedNodes(SpecialMemberKey)
                .Where(p => p.GetSpecialMemberAnnotation() == specialMember);
    }
}

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Generation
{
    public static class ResourceSyntaxNodeExtensions
    {
        private const string ResourceNameKey = "ResourceName";

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
    }
}

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class PropertyHelpers
    {
        /// <summary>
        /// Makes the type of a <see cref="PropertyDeclarationSyntax"/> nullable, if it isn't already.
        /// </summary>
        /// <param name="property">The <see cref="PropertyDeclarationSyntax"/> to update.</param>
        /// <returns>The mutated property declaration, or the original if no mutation was required.</returns>
        [Pure]
        public static PropertyDeclarationSyntax MakeNullable(this PropertyDeclarationSyntax property) =>
            property.Type is NullableTypeSyntax
                ? property // Already nullable
                : property.WithType(NullableType(property.Type));

        /// <summary>
        /// If the given property is a reference type and is not initialized, it should *either* be marked as nullable or
        /// be initialized to a non-null value. This method will do it's best to initialize the property using a default constructor
        /// or empty string, and failing that will mark the type as nullable.
        /// </summary>
        /// <param name="property">The <see cref="PropertyDeclarationSyntax"/> to update. Must be on a <see cref="SyntaxTree"/>.</param>
        /// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
        /// <returns>The mutated property declaration, or the original if no mutation was required.</returns>
        [Pure]
        public static PropertyDeclarationSyntax MakeNullableOrInitializeIfReferenceType(this PropertyDeclarationSyntax property,
            CSharpCompilation compilation) =>
            MakeNullableOrInitializeIfReferenceType(property, compilation.GetSemanticModel(property.SyntaxTree));

        /// <summary>
        /// If the given property is a reference type and is not initialized, it should *either* be marked as nullable or
        /// be initialized to a non-null value. This method will do it's best to initialize the property using a default constructor
        /// or empty string, and failing that will mark the type as nullable.
        /// </summary>
        /// <param name="property">The <see cref="PropertyDeclarationSyntax"/> to update. Must be on a <see cref="SyntaxTree"/>.</param>
        /// <param name="semanticModel"><see cref="SemanticModel"/> used to perform type analysis.</param>
        /// <returns>The mutated property declaration, or the original if no mutation was required.</returns>
        [Pure]
        public static PropertyDeclarationSyntax MakeNullableOrInitializeIfReferenceType(this PropertyDeclarationSyntax property,
            SemanticModel semanticModel)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (semanticModel == null)
            {
                throw new ArgumentNullException(nameof(semanticModel));
            }

            if (property.Initializer != null || property.ExpressionBody != null)
            {
                // No need if already initialized or expression body only
                return property;
            }

            if (property.Type is NullableTypeSyntax)
            {
                // Already nullable
                return property;
            }

            var typeInfo = semanticModel.GetTypeInfo(property.Type);
            if (typeInfo.Type?.IsReferenceType ?? false)
            {
                if (typeInfo.Type.SpecialType == SpecialType.System_String)
                {
                    // Initialize to an empty string

                    property = property
                        .WithInitializer(EqualsValueClause(SyntaxHelpers.StringLiteral("")))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }
                else if (!typeInfo.Type.IsAbstract && typeInfo.Type.GetMembers()
                    .Where(p => p.Kind == SymbolKind.Method && p.Name == ".ctor")
                    .Cast<IMethodSymbol>()
                    .Any(p => p.Parameters.Length == 0 && p.DeclaredAccessibility == Accessibility.Public))
                {
                    // Build a default object using the default constructor

                    property = property
                        .WithInitializer(EqualsValueClause(ObjectCreationExpression(property.Type)))
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
                }
                else
                {
                    // Mark the types as nullable, even if the parameter is required
                    // This will encourage SDK consumers to check for nulls and prevent NREs

                    property = property.MakeNullable();
                }
            }

            return property;
        }
    }
}

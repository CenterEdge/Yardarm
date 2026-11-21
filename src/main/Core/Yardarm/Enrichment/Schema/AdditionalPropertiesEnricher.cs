using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Schema
{
    /// <summary>
    /// Adds AdditionalProperties to object schemas, but runs after the <see cref="BaseTypeEnricher"/>. This allows
    /// property shadowing with the new keyword or dropping of unnecessary duplicates.
    /// </summary>
    public class AdditionalPropertiesEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, IOpenApiSchema>
    {
        private readonly GenerationContext _context;

        public Type[] ExecuteAfter { get; } =
        {
            typeof(BaseTypeEnricher)
        };

        public AdditionalPropertiesEnricher(GenerationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSchema> context)
        {
            if (!context.Element.AllowsUndeclaredProperties)
            {
                return target;
            }

            (TypeSyntax dictionaryType, TypeSyntax interfaceType, TypeSyntax valueType) = GetDictionaryType(context);

            string propertyName = _context.NameFormatterSelector.GetFormatter(NameKind.Property)
                .Format("AdditionalProperties");
            bool isShadowing = false;

            var originalNode = context.OriginalNode as ClassDeclarationSyntax;

            // Check to see if we're inheriting from a type that already implements AdditionalProperties. The nearest
            // ancestor that allows undeclared properties declares the dictionary that this type inherits. Ancestors
            // between it and this type, such as an abstract or closed schema, declare none.
            if (originalNode is not null
                && FindNearestAncestorAllowingUndeclaredProperties(context, originalNode) is { } ancestorSchema)
            {
                var ancestorAdditionalPropertiesTypeGenerator =
                    _context.TypeGeneratorRegistry.Get(ancestorSchema.GetAdditionalPropertiesOrDefault());

                if (ancestorAdditionalPropertiesTypeGenerator.TypeInfo.Name.IsEquivalentTo(valueType))
                {
                    // The types match, we can just accept the inherited version
                    return target;
                }

                // We must shadow
                isShadowing = true;
            }

            return target.AddMembers(GenerateAdditionalPropertiesMembers(dictionaryType, interfaceType,
                propertyName, isShadowing));
        }

        /// <summary>
        /// Walks the base class chain of <paramref name="classDeclaration"/> and returns the schema of the nearest
        /// ancestor that allows undeclared properties, and so declares an <c>AdditionalProperties</c> dictionary.
        /// Ancestors without a schema, such as <see cref="object"/>, are skipped.
        /// </summary>
        private ILocatedOpenApiElement<IOpenApiSchema>? FindNearestAncestorAllowingUndeclaredProperties(
            OpenApiEnrichmentContext<IOpenApiSchema> context, ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration.BaseList is not {Types.Count: > 0})
            {
                return null;
            }

            var semanticModel = context.Compilation.GetSemanticModel(context.SyntaxTree);

            INamedTypeSymbol? ancestor = semanticModel.GetDeclaredSymbol(classDeclaration)?.BaseType;
            while (ancestor is {TypeKind: TypeKind.Class})
            {
                var ancestorSchema = ancestor
                    .DeclaringSyntaxReferences.FirstOrDefault()?
                    .GetSyntax().GetElementAnnotation<IOpenApiSchema>(_context.ElementRegistry);

                if (ancestorSchema is not null && ancestorSchema.Element.AllowsUndeclaredProperties)
                {
                    return ancestorSchema;
                }

                ancestor = ancestor.BaseType;
            }

            return null;
        }

        private (TypeSyntax dictionaryType, TypeSyntax interfaceType, TypeSyntax valueType) GetDictionaryType(OpenApiEnrichmentContext<IOpenApiSchema> context)
        {
            ILocatedOpenApiElement<IOpenApiSchema> additionalProperties =
                context.LocatedElement.GetAdditionalPropertiesOrDefault();
            ITypeGenerator schemaGenerator = _context.TypeGeneratorRegistry.Get(additionalProperties);

            TypeSyntax valueType = schemaGenerator.TypeInfo.Name;
            if (additionalProperties.Element.Nullable)
            {
                valueType = NullableType(valueType);
            }

            var dictionaryType = WellKnownTypes.System.Collections.Generic.DictionaryT.Name(
                PredefinedType(Token(SyntaxKind.StringKeyword)), valueType);

            var interfaceType = WellKnownTypes.System.Collections.Generic.IDictionaryT.Name(
                PredefinedType(Token(SyntaxKind.StringKeyword)), valueType);

            return (dictionaryType, interfaceType, valueType);
        }

        private MemberDeclarationSyntax GenerateAdditionalPropertiesMembers(TypeSyntax dictionaryType, TypeSyntax interfaceType,
            string propertyName, bool isShadowing)
        {
            var property = PropertyDeclaration(
                    default,
                    TokenList(Token(SyntaxKind.PublicKeyword)),
                    interfaceType,
                    null,
                    Identifier(propertyName),
                    AccessorList(SingletonList(AccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration,
                        default,
                        default,
                        Token(SyntaxKind.GetKeyword),
                        null,
                        null,
                        Token(SyntaxKind.SemicolonToken)))),
                    null,
                    EqualsValueClause(ObjectCreationExpression(dictionaryType, ArgumentList(), null)),
                    Token(SyntaxKind.SemicolonToken))
                .AddSpecialMemberAnnotation(SpecialMembers.AdditionalProperties);

            if (isShadowing)
            {
                property = property.AddModifiers(Token(SyntaxKind.NewKeyword));
            }

            return property;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
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
    public class AdditionalPropertiesEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, OpenApiSchema>
    {
        private readonly GenerationContext _context;

        public int Priority => 1;

        public AdditionalPropertiesEnricher(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            if (!context.Element.AdditionalPropertiesAllowed)
            {
                return target;
            }

            (TypeSyntax dictionaryType, TypeSyntax interfaceType) = GetDictionaryType(context);

            string propertyName = _context.NameFormatterSelector.GetFormatter(NameKind.Property)
                .Format("AdditionalProperties");
            bool isShadowing = false;

            // Check to see if we're inheriting from a type that already implements AdditionalProperties
            if (target.BaseList != null)
            {
                var semanticModel = context.Compilation.GetSemanticModel(context.SyntaxTree);

                foreach (BaseTypeSyntax baseType in target.BaseList.Types)
                {
                    var typeInfo = ModelExtensions.GetTypeInfo(semanticModel, baseType.Type);
                    if (typeInfo.Type?.TypeKind == TypeKind.Class)
                    {
                        if (typeInfo.Type.GetMembers(propertyName)
                            .OfType<IPropertySymbol>().FirstOrDefault()?
                            .DeclaringSyntaxReferences.FirstOrDefault()?
                            .GetSyntax() is PropertyDeclarationSyntax baseMember)
                        {
                            if (baseMember.Type.IsEquivalentTo(interfaceType))
                            {
                                // The types match, we can just accept the inherited version
                                return target;
                            }
                            else
                            {
                                // We must shadow
                                isShadowing = true;
                                break;
                            }
                        }
                    }
                }
            }

            return target.AddMembers(GenerateAdditionalPropertiesMembers(dictionaryType, interfaceType,
                propertyName, isShadowing));
        }

        private (TypeSyntax dictionartyType, TypeSyntax interfaceType) GetDictionaryType(OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            ILocatedOpenApiElement<OpenApiSchema> additionalProperties =
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

            return (dictionaryType, interfaceType);
        }

        private MemberDeclarationSyntax GenerateAdditionalPropertiesMembers(TypeSyntax dictionaryType, TypeSyntax interfaceType,
            string propertyName, bool isShadowing)
        {
            var property = PropertyDeclaration(interfaceType, Identifier(propertyName))
                .AddSpecialMemberAnnotation(SpecialMembers.AdditionalProperties)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                .WithInitializer(EqualsValueClause(ObjectCreationExpression(dictionaryType)));

            if (isShadowing)
            {
                property = property.AddModifiers(Token(SyntaxKind.NewKeyword));
            }

            return property;
        }
    }
}

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            if (!context.Element.AdditionalPropertiesAllowed)
            {
                return target;
            }

            (TypeSyntax dictionaryType, TypeSyntax interfaceType, TypeSyntax valueType) = GetDictionaryType(context);

            string propertyName = _context.NameFormatterSelector.GetFormatter(NameKind.Property)
                .Format("AdditionalProperties");
            bool isShadowing = false;

            var originalNode = context.OriginalNode as ClassDeclarationSyntax;

            // Check to see if we're inheriting from a type that already implements AdditionalProperties
            if (originalNode?.BaseList is {Types.Count: > 0})
            {
                var semanticModel = context.Compilation.GetSemanticModel(context.SyntaxTree);

                foreach (BaseTypeSyntax baseType in originalNode.BaseList.Types)
                {
                    var typeInfo = ModelExtensions.GetTypeInfo(semanticModel, baseType.Type);
                    if (typeInfo.Type?.TypeKind == TypeKind.Class)
                    {
                        var parentSchema = typeInfo.Type
                            .DeclaringSyntaxReferences.FirstOrDefault()?
                            .GetSyntax().GetElementAnnotation<OpenApiSchema>(_context.ElementRegistry);

                        if (parentSchema is not null && parentSchema.Element.AdditionalPropertiesAllowed)
                        {
                            // We will already have an AdditionalProperties property on the base type

                            var parentAdditionalPropertiesTypeGenerator =
                                _context.TypeGeneratorRegistry.Get(parentSchema.GetAdditionalPropertiesOrDefault());

                            if (parentAdditionalPropertiesTypeGenerator.TypeInfo.Name.IsEquivalentTo(valueType))
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

        private (TypeSyntax dictionaryType, TypeSyntax interfaceType, TypeSyntax valueType) GetDictionaryType(OpenApiEnrichmentContext<OpenApiSchema> context)
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

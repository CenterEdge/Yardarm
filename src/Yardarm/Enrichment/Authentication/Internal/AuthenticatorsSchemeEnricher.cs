using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Authentication.Internal
{
    /// <summary>
    /// Adds properties to the Authenticators class for each configured security scheme.
    /// </summary>
    internal class AuthenticatorsSchemeEnricher : IResourceFileEnricher
    {
        private readonly GenerationContext _context;

        public int Priority => 0;

        public AuthenticatorsSchemeEnricher(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public bool ShouldEnrich(string resourceName) =>
            resourceName == "Yardarm.Client.Authentication.Authenticators.cs";

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target, ResourceFileEnrichmentContext context)
        {
            ClassDeclarationSyntax? classDeclaration = target.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.ValueText == "Authenticators");

            if (classDeclaration == null)
            {
                return target;
            }

            return target.ReplaceNode(
                classDeclaration,
                classDeclaration.AddMembers(
                    GenerateProperties().ToArray<MemberDeclarationSyntax>()));
        }

        public IEnumerable<PropertyDeclarationSyntax> GenerateProperties()
        {
            var nameFormatter = _context.NameFormatterSelector.GetFormatter(NameKind.Property);

            foreach (var scheme in _context.Document.Components.SecuritySchemes.Select(p => p.Value.CreateRoot(p.Key)))
            {
                TypeSyntax typeName = _context.TypeGeneratorRegistry.Get(scheme).TypeInfo.Name;

                string propertyName = nameFormatter.Format(scheme.Key);

                yield return PropertyDeclaration(NullableType(typeName), propertyName)
                    .AddModifiers(Token(SyntaxKind.PublicKeyword))
                    .AddAccessorListAccessors(
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
            }
        }
    }
}

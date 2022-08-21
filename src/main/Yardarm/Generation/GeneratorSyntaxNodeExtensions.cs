using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Yardarm.Generation
{
    public static class GeneratorSyntaxNodeExtensions
    {
        public const string GeneratorAnnotationName = "YardarmGenerator";

        public static TSyntaxNode AddGeneratorAnnotation<TSyntaxNode>(this TSyntaxNode node,
            TypeGeneratorBase generator)
            where TSyntaxNode : SyntaxNode =>
            node.AddGeneratorAnnotation(generator.GetType());

        public static TSyntaxNode AddGeneratorAnnotation<TSyntaxNode>(this TSyntaxNode node,
            ISyntaxTreeGenerator generator)
            where TSyntaxNode : SyntaxNode =>
            node.AddGeneratorAnnotation(generator.GetType());

        public static TSyntaxNode AddGeneratorAnnotation<TSyntaxNode>(this TSyntaxNode node,
            Type generatorType)
            where TSyntaxNode : SyntaxNode =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(GeneratorAnnotationName, generatorType.FullName));

        public static Type? GetGeneratorAnnotation(this SyntaxNode node)
        {
            string? typeName = node.GetAnnotations(GeneratorAnnotationName).FirstOrDefault()?.Data;
            if (typeName == null)
            {
                return null;
            }

            return Type.GetType(typeName, false);
        }
    }
}

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Yardarm.Generation;

public static class GeneratorSyntaxNodeExtensions
{
    public const string GeneratorAnnotationName = "YardarmGenerator";

    extension<TSyntaxNode>(TSyntaxNode node)
        where TSyntaxNode : SyntaxNode
    {
        public TSyntaxNode AddGeneratorAnnotation(TypeGeneratorBase generator) =>
            node.AddGeneratorAnnotation(generator.GetType());

        public TSyntaxNode AddGeneratorAnnotation(ISyntaxTreeGenerator generator) =>
            node.AddGeneratorAnnotation(generator.GetType());

        public TSyntaxNode AddGeneratorAnnotation(Type generatorType) =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(GeneratorAnnotationName, generatorType.FullName));
    }

    extension(SyntaxNode node)
    {
        public Type? GetGeneratorAnnotation()
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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Helpers;

public static class DiagnosticsExtensions
{
    extension(Diagnostic diagnostic)
    {
        public string? GetSource(IOpenApiElementRegistry elementRegistry)
        {
            ArgumentNullException.ThrowIfNull(diagnostic);
            ArgumentNullException.ThrowIfNull(elementRegistry);

            SyntaxTree? syntaxTree = diagnostic.Location.SourceTree;
            if (syntaxTree == null)
            {
                return null;
            }

            CompilationUnitSyntax compilationUnit = syntaxTree.GetCompilationUnitRoot();

            return compilationUnit.GetResourceNameAnnotation()
                   ?? compilationUnit.GetIncludedFileNameAnnotation()
                   ?? compilationUnit.GetElementAnnotations(elementRegistry).FirstOrDefault()?.ToString();
        }

        public string GetMessageWithSource(IOpenApiElementRegistry elementRegistry)
        {
            string? source = diagnostic.GetSource(elementRegistry);

            return source == null
                ? diagnostic.ToString()
                : $"{source} {diagnostic}";
        }
    }
}

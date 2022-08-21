using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Helpers
{
    public static class DiagnosticsExtensions
    {
        public static string? GetSource(this Diagnostic diagnostic, IOpenApiElementRegistry elementRegistry)
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }
            if (elementRegistry == null)
            {
                throw new ArgumentNullException(nameof(elementRegistry));
            }

            SyntaxTree? syntaxTree = diagnostic.Location.SourceTree;
            if (syntaxTree == null)
            {
                return null;
            }

            CompilationUnitSyntax compilationUnit = syntaxTree.GetCompilationUnitRoot();

            return compilationUnit.GetResourceNameAnnotation()
                   ?? compilationUnit.GetElementAnnotations(elementRegistry).FirstOrDefault()?.ToString();
        }

        public static string GetMessageWithSource(this Diagnostic diagnostic, IOpenApiElementRegistry elementRegistry)
        {
            string? source = diagnostic.GetSource(elementRegistry);

            return source == null
                ? diagnostic.ToString()
                : $"{source} {diagnostic}";
        }
    }
}

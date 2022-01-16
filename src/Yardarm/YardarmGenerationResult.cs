using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace Yardarm
{
    public class YardarmGenerationResult
    {
        public GenerationContext Context { get; }

        public EmitResult CompilationResult { get; }

        public ImmutableArray<Diagnostic>? AdditionalDiagnostics { get; }

        public bool Success => CompilationResult.Success;

        public YardarmGenerationResult(GenerationContext context, EmitResult compilationResult, ImmutableArray<Diagnostic>? additionalDiagnostics = null)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            CompilationResult = compilationResult ?? throw new ArgumentNullException(nameof(compilationResult));
            AdditionalDiagnostics = additionalDiagnostics;
        }

        public IEnumerable<Diagnostic> GetAllDiagnostics()
        {
            if (AdditionalDiagnostics is not null)
            {
                return AdditionalDiagnostics.Concat(CompilationResult.Diagnostics);
            }
            else
            {
                return CompilationResult.Diagnostics;
            }
        }
    }
}

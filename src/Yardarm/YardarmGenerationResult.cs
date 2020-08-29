using System;
using Microsoft.CodeAnalysis.Emit;

namespace Yardarm
{
    public class YardarmGenerationResult
    {
        public GenerationContext Context { get; }

        public EmitResult CompilationResult { get; }

        public bool Success => CompilationResult.Success;

        public YardarmGenerationResult(GenerationContext context, EmitResult compilationResult)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            CompilationResult = compilationResult ?? throw new ArgumentNullException(nameof(compilationResult));
        }
    }
}

using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using NuGet.Frameworks;

namespace Yardarm
{
    public class YardarmCompilationResult
    {
        public NuGetFramework TargetFramework { get; set; }
        public EmitResult EmitResult { get; }
        internal Stream DllOutput { get; }
        internal Stream PdbOutput { get; }
        internal Stream XmlDocumentationOutput { get; }
        public ImmutableArray<Diagnostic> AdditionalDiagnostics { get; }

        internal YardarmCompilationResult(NuGetFramework targetFramework, EmitResult emitResult, Stream dllOutput, Stream pdbOutput,
            Stream xmlDocumentationOutput, ImmutableArray<Diagnostic> additionalDiagnostics)
        {
            TargetFramework = targetFramework;
            EmitResult = emitResult;
            DllOutput = dllOutput;
            PdbOutput = pdbOutput;
            XmlDocumentationOutput = xmlDocumentationOutput;
            AdditionalDiagnostics = additionalDiagnostics;
        }
    }
}

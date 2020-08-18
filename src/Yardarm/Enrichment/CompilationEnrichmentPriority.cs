namespace Yardarm.Enrichment
{
    public static class CompilationEnrichmentPriority
    {
        public const int References = 0;

        public const int SyntaxTreeGeneration = 100;

        public const int WellKnownTypeEnrichment = 200;

        public const int OpenApiSyntaxTreeEnrichment = 300;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Yardarm.Generation;
using Yardarm.Names;

namespace Yardarm.NodaTime.Internal;

internal sealed partial class ClientGenerator(GenerationContext generationContext, IRootNamespace rootNamespace)
    : ResourceSyntaxTreeGenerator(generationContext, rootNamespace)
{
    protected override string ResourcePrefix => "Yardarm.NodaTime.Client.";

    public override IEnumerable<Regex> GetResourceNameExclusions()
    {
        foreach (Regex exclusion in base.GetResourceNameExclusions())
        {
            yield return exclusion;
        }

        if (!generationContext.Settings.Extensions.Any(p => p.Name == "SystemTextJsonExtension"))
        {
            yield return NodaTimeJsonConverterFactory();
        }
    }

    [GeneratedRegex(@"YardarmNodaTimeJsonConverterFactory\.cs$")]
    private static partial Regex NodaTimeJsonConverterFactory();
}

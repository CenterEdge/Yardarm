using Yardarm.Generation;
using Yardarm.Names;

namespace Yardarm.NodaTime.Internal;

internal sealed class ClientGenerator(GenerationContext generationContext, IRootNamespace rootNamespace)
    : ResourceSyntaxTreeGenerator(generationContext, rootNamespace)
{
    protected override string ResourcePrefix => "Yardarm.NodaTime.Client.";
}

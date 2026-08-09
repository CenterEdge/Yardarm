using Yardarm.Generation;
using Yardarm.Names;

namespace Yardarm.SystemTextJson.Internal;

internal class ClientGenerator(GenerationContext generationContext, IRootNamespace rootNamespace)
    : ResourceSyntaxTreeGenerator(generationContext, rootNamespace)
{
    protected override string ResourcePrefix => "Yardarm.SystemTextJson.Client.";
}

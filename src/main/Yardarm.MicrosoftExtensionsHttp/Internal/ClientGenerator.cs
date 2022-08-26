using Yardarm.Generation;
using Yardarm.Names;

namespace Yardarm.MicrosoftExtensionsHttp.Internal
{
    internal class ClientGenerator : ResourceSyntaxTreeGenerator
    {
        protected override string ResourcePrefix => "Yardarm.MicrosoftExtensionsHttp.Client.";

        public ClientGenerator(GenerationContext generationContext, IRootNamespace rootNamespace)
            : base(generationContext, rootNamespace)
        {
        }
    }
}

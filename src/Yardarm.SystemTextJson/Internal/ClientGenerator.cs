using Yardarm.Generation;
using Yardarm.Names;

namespace Yardarm.SystemTextJson.Internal
{
    internal class ClientGenerator : ResourceSyntaxTreeGenerator
    {
        protected override string ResourcePrefix => "Yardarm.SystemTextJson.Client.";

        public ClientGenerator(IRootNamespace rootNamespace)
            : base(rootNamespace)
        {
        }
    }
}

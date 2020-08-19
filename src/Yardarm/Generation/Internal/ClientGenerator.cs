using Yardarm.Names;

namespace Yardarm.Generation.Internal
{
    internal class ClientGenerator : ResourceSyntaxTreeGenerator
    {
        protected override string ResourcePrefix => "Yardarm.Client.";

        public ClientGenerator(IRootNamespace rootNamespace)
            : base(rootNamespace)
        {
        }
    }
}

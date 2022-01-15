using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Yardarm.Internal
{
    /// <summary>
    /// Provides a mechanism for loading assemblies temporarily during a Yardarm execution and
    /// unloading them afterwards.
    /// </summary>
    internal class YardarmAssemblyLoadContext : AssemblyLoadContext
    {
        public YardarmAssemblyLoadContext() : base(isCollectible: true)
        {
        }

        protected override Assembly? Load(AssemblyName assemblyName) => null;
    }
}

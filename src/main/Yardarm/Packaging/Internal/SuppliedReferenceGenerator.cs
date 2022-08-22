using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Yardarm.Generation;

namespace Yardarm.Packaging.Internal
{
    /// <summary>
    /// Implementation of IReferenceGenerator used when references are supplied externally, usually by MSBuild.
    /// </summary>
    public class SuppliedReferenceGenerator : IReferenceGenerator
    {
        private readonly YardarmGenerationSettings _settings;

        public SuppliedReferenceGenerator(YardarmGenerationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            _settings = settings;
        }

        public IAsyncEnumerable<MetadataReference> Generate(CancellationToken cancellationToken = default)
        {
            var assemblies = _settings.ReferencedAssemblies;
            if (assemblies is null)
            {
                return AsyncEnumerable.Empty<MetadataReference>();
            }

            return assemblies.Select(p => MetadataReference.CreateFromFile(p)).ToAsyncEnumerable();
        }
    }
}

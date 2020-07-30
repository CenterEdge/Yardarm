using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Yardarm.Generation
{
    public interface IReferenceGenerator
    {
        IEnumerable<MetadataReference> Generate();
    }
}

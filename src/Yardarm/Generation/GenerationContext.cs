using System;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation
{
    public class GenerationContext
    {
        public OpenApiDocument Document { get; set; }

        public GenerationContext(OpenApiDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }
    }
}

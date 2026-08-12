using System;
using Newtonsoft.Json.Converters;

namespace RootNamespace.Serialization.Json
{
    /// <summary>
    /// Handles reading and writing date-only JSON to and from <see cref="DateTime"/> properties.
    /// </summary>
    internal class OpenApiDateConverter : IsoDateTimeConverter
    {
        public OpenApiDateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}

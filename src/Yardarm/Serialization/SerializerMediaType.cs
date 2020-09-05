using System;

namespace Yardarm.Serialization
{
    public class SerializerMediaType
    {
        public string MediaType { get; }

        public double Quality { get; }

        public SerializerMediaType(string mediaType, double quality)
        {
            if (quality < 0 || quality > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quality), $"{nameof(quality)} must be between 0 and 1, inclusive.");
            }

            MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
            Quality = quality;
        }
    }
}

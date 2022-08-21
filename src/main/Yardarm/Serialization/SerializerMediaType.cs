using System;

namespace Yardarm.Serialization
{
    public class SerializerMediaType : IEquatable<SerializerMediaType>
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

        public bool Equals(SerializerMediaType? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return MediaType == other.MediaType && Quality.Equals(other.Quality);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((SerializerMediaType) obj);
        }

        public override int GetHashCode() => HashCode.Combine(MediaType, Quality);
    }
}

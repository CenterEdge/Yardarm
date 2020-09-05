namespace Yardarm.Serialization
{
    public struct SerializerDescriptorWithPriority
    {
        public SerializerDescriptor Descriptor { get; set; }

        public double Quality { get; set; }
    }
}

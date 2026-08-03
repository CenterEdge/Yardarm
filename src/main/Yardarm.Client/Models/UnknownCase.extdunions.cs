namespace RootNamespace.Models
{
    /// <summary>
    /// May be added to a union type to allow deserialization of unknown union cases to a placeholder type, rather than throwing an exception.
    /// </summary>
    public sealed class UnknownCase
    {
        /// <summary>
        /// Singleton instance of <see cref="UnknownCase"/>. This is the only instance of this type, and is used to indicate that a union case is unknown.
        /// </summary>
        public static UnknownCase Value { get; } = new();

        // Prevent construction outside of this class, so that the only instance is the singleton Value property.
        private UnknownCase()
        {
        }

        public override string ToString() => nameof(UnknownCase);
    }
}

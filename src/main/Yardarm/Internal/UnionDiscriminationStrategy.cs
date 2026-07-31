namespace Yardarm.Internal;

internal enum UnionDiscriminationStrategy
{
    /// <summary>
    /// Does not use unions.
    /// </summary>
    None,

    /// <summary>
    /// Uses externally discriminated unions, where the discriminator is a property of the object
    /// and the case is serialized as the value of that property.
    /// </summary>
    External,
}

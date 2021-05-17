using System;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Annotates a property with the name to use when encoded as multipart/form-data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class MultipartPropertyAttribute : Attribute
    {
        /// <summary>
        /// Name of the property when serialized.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a new MultipartPropertyAttribute.
        /// </summary>
        /// <param name="name">Name of the property when serialized.</param>
        public MultipartPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}

using System;

namespace RootNamespace.Serialization
{
    /// <summary>
    /// Decorates a multipart encoded request class to provide the media types
    /// which are acceptable on each property in the request body. One attribute
    /// should be applied per property.
    /// </summary>
    /// <remarks>
    /// This class is placed on the request class rather than the schema property to
    /// align with OpenAPI 3. The encoding information is part of the request media
    /// type, not part of the schema. The same schema could be used with multiple
    /// requests with different media types.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MultipartEncodingAttribute : Attribute
    {
        /// <summary>
        /// Name of the property described by this attribute.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// List of acceptable media types.
        /// </summary>
        public string[] MediaTypes { get; set; }

        public MultipartEncodingAttribute(string propertyName, params string[] mediaTypes)
        {
            PropertyName = propertyName;
            MediaTypes = mediaTypes;
        }
    }
}

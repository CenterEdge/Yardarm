namespace RootNamespace.Serialization;

/// <summary>
/// Provides additional details about the value of a multipart encoded field.
/// </summary>
public class MultipartFieldDetails
{
    /// <summary>
    /// Optional Content-Type of the field. If null, a default value is used.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Optional file name of the field to apply within Content-Disposition.
    /// </summary>
    public string? Filename { get; set; }
}

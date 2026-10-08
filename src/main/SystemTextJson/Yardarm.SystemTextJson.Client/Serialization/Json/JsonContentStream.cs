using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json;

internal static class JsonContentStream
{
    /// <summary>
    /// Gets the content as a stream of UTF-8, which is what <c>System.Text.Json</c> reads. If the content declares a
    /// different charset, it is transcoded as it is read, without buffering the whole content.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// The charset of the content is not supported. Before .NET 5, only UTF-8 is supported.
    /// </exception>
    public static async Task<Stream> ReadAsUtf8StreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
        Encoding? encoding = GetEncoding(content);

#if !NET5_0_OR_GREATER
        if (encoding is not null)
        {
            // Encoding.CreateTranscodingStream isn't available, so only UTF-8 can be read
            throw new NotSupportedException(
                $"The character set '{encoding.WebName}' is not supported on this runtime, only UTF-8 is.");
        }
#endif

        Stream stream = await ReadAsStreamAsync(content, cancellationToken).ConfigureAwait(false);

#if NET5_0_OR_GREATER
        return encoding is null
            ? stream
            : Encoding.CreateTranscodingStream(stream, encoding, Encoding.UTF8, leaveOpen: false);
#else
        return stream;
#endif
    }

    // Returns null if the content is UTF-8 and no transcoding is required
    private static Encoding? GetEncoding(HttpContent content)
    {
        string? charset = content.Headers.ContentType?.CharSet;
        if (string.IsNullOrEmpty(charset))
        {
            return null;
        }

        // The charset may be quoted
        charset = charset!.Trim('"');

        try
        {
            Encoding encoding = Encoding.GetEncoding(charset);

            return encoding.CodePage == Encoding.UTF8.CodePage ? null : encoding;
        }
        catch (ArgumentException ex)
        {
            throw new NotSupportedException($"The character set '{charset}' is not supported.", ex);
        }
    }

    private static Task<Stream> ReadAsStreamAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStreamAsync(cancellationToken);
#else
        cancellationToken.ThrowIfCancellationRequested();
        return content.ReadAsStreamAsync();
#endif
    }
}

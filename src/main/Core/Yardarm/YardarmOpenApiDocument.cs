using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;
using Yardarm.Spec;

namespace Yardarm;

/// <summary>
/// Provides Yardarm-specific OpenAPI document loading behavior.
/// </summary>
public static class YardarmOpenApiDocument
{
    public static async Task<OpenApiDocument> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();

        var result = await OpenApiDocument.LoadAsync(stream, settings: settings, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var document = result.Document ?? throw new InvalidDataException(
            "The OpenAPI document could not be read.");

        var diagnostic = result.Diagnostic ?? throw new InvalidDataException(
            "The OpenAPI document did not provide parsing diagnostics.");

        OpenApiAdditionalOperationsConverter.Convert(
            document,
            diagnostic.SpecificationVersion,
            settings);

        return document;
    }
}

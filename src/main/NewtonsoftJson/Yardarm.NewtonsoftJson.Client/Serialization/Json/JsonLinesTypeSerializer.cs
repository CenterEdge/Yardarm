using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json
{
    /// <summary>
    /// Serializes JSON Lines (newline-delimited JSON) bodies. Collections are written and read as one JSON
    /// value per line; other values are written and read as a single line.
    /// </summary>
    public class JsonLinesTypeSerializer : ITypeSerializer
    {
        private static readonly UTF8Encoding s_utf8NoBomEncoding = new(false);

        public static string[] SupportedMediaTypes =>
        [
            "application/jsonl",
            "application/x-ndjson"
        ];

        private readonly JsonSerializer _serializer;

        public JsonLinesTypeSerializer()
            : this(JsonTypeSerializer.CreateDefaultSettings())
        {
        }

        public JsonLinesTypeSerializer(JsonSerializerSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            _serializer = JsonSerializer.Create(settings);

            // Records must not span lines. The serializer formatting overrides the writer formatting,
            // so this also overrides indented settings.
            _serializer.Formatting = Formatting.None;
        }

        public HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null) =>
            new JsonLinesContent(value, typeof(T), _serializer,
                new MediaTypeHeaderValue(mediaType) { CharSet = s_utf8NoBomEncoding.WebName });

        public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null) =>
            DeserializeAsync<T>(content, serializationData, default);

        public async ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);

#if NET5_0_OR_GREATER
            Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
            cancellationToken.ThrowIfCancellationRequested();
            Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

            // Reads are synchronous, the response content is already buffered
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            using var reader = new JsonTextReader(streamReader);
            reader.SupportMultipleContent = true;

            JsonContract contract = _serializer.ContractResolver.ResolveContract(typeof(T));
            if (contract is not JsonArrayContract arrayContract)
            {
                return _serializer.Deserialize<T>(reader)!;
            }

            if (arrayContract.DefaultCreator?.Invoke() is not IList list)
            {
                throw new NotSupportedException($"JSON Lines deserialization of {typeof(T)} requires a type that implements {nameof(IList)} with a default constructor.");
            }

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.TokenType == JsonToken.Comment)
                {
                    continue;
                }

                list.Add(_serializer.Deserialize(reader, arrayContract.CollectionItemType));
            }

            return (T)list;
        }

        private sealed class JsonLinesContent : HttpContent
        {
            private readonly object? _value;
            private readonly Type _type;
            private readonly JsonSerializer _serializer;

            public JsonLinesContent(object? value, Type type, JsonSerializer serializer, MediaTypeHeaderValue mediaType)
            {
                _value = value;
                _type = type;
                _serializer = serializer;

                Headers.ContentType = mediaType;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
                SerializeToStreamAsync(stream, CancellationToken.None);

#if NET5_0_OR_GREATER
            protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context,
                CancellationToken cancellationToken) =>
                SerializeToStreamAsync(stream, cancellationToken);
#endif

            private async Task SerializeToStreamAsync(Stream stream, CancellationToken cancellationToken)
            {
                using var streamWriter = new StreamWriter(stream, s_utf8NoBomEncoding, 1024, leaveOpen: true);
                using var jsonWriter = new JsonTextWriter(streamWriter);
                jsonWriter.Formatting = Formatting.None;
                jsonWriter.CloseOutput = false;

                if (_serializer.ContractResolver.ResolveContract(_type) is JsonArrayContract arrayContract)
                {
                    if (_value is IEnumerable items)
                    {
                        foreach (object? item in items)
                        {
                            await WriteLineAsync(jsonWriter, item, arrayContract.CollectionItemType, cancellationToken)
                                .ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await WriteLineAsync(jsonWriter, _value, _type, cancellationToken).ConfigureAwait(false);
                }
            }

            private async Task WriteLineAsync(JsonTextWriter jsonWriter, object? value, Type? type,
                CancellationToken cancellationToken)
            {
                _serializer.Serialize(jsonWriter, value, type);
                jsonWriter.WriteRaw("\n");
                await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }
    }
}

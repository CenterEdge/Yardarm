using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class PlainTextSerializer : ITypeSerializer
    {
        private static readonly MethodInfo _deserializeStringMethod =
            typeof(PlainTextSerializer)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(p => p.Name == nameof(Deserialize) && p.GetParameters()[0].ParameterType == typeof(string));

        public static string[] SupportedMediaTypes => new [] { MediaTypeNames.Text.Plain };

        public HttpContent Serialize<T>(T value, string mediaType) =>
            new StringContent(Serialize<T>(value), Encoding.UTF8, mediaType);

        public string Serialize<T>(T value) => value?.ToString() ?? "";

        public async ValueTask<T> DeserializeAsync<T>(HttpContent content)
        {
            string value = await content.ReadAsStringAsync().ConfigureAwait(false);

            return Deserialize<T>(value);
        }

        [return: MaybeNull]
        public T Deserialize<T>(IEnumerable<string> values)
        {
            Type type = typeof(T);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type itemType = type.GetGenericArguments()[0];
                MethodInfo deserializeMethod = _deserializeStringMethod.MakeGenericMethod(itemType);

                return (T)(object) values
                    .Select(p => deserializeMethod.Invoke(null, new object[] {p}))
                    .ToList();
            }
            else
            {
                string? value = values.FirstOrDefault();
                if (value == null)
                {
                    return default;
                }
                else
                {
                    return Deserialize<T>(value);
                }
            }
        }

        private static T Deserialize<T>(string value) =>
            (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(value)!;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    internal ref struct QueryStringBuilder
    {
        private readonly StringBuilder _stringBuilder;
        private bool _hasFirstParameter;

        public QueryStringBuilder(string uri)
        {
            _stringBuilder = new StringBuilder(uri);
            _hasFirstParameter = false;
        }

        public override string ToString() => _stringBuilder.ToString();

        /// <summary>
        /// Append a primitive.
        /// </summary>
        /// <typeparam name="T">Type of primitive.</typeparam>
        /// <param name="name">Name of the query parameter. Should be URL encoded.</param>
        /// <param name="value">Value to serialize.</param>
        /// <param name="allowReserved">If true, do not URL encode the serialized value.</param>
        /// <param name="format">Open API format specifier, i.e. "date" or "date-time".</param>
        /// <remarks>
        /// Null values are ignored.
        /// </remarks>
        public void AppendPrimitive<T>(string name, T value, bool allowReserved, string? format = null)
        {
            if (value is null)
            {
                return;
            }

            Append(name, EscapeValue(value, allowReserved, format));
        }

        /// <summary>
        /// Append a list of primitives.
        /// </summary>
        /// <typeparam name="T">Type of primitive.</typeparam>
        /// <param name="name">Name of the query parameter. Should be URL encoded.</param>
        /// <param name="list">List of values to serialize.</param>
        /// <param name="explode">If true, add a separate query parameter for each item in the list.</param>
        /// <param name="delimiter">If <paramref name="explode"/> is <c>false</c>, this string separates each value.</param>
        /// <param name="allowReserved">If true, do not URL encode the serialized values.</param>
        /// <param name="format">Open API format specifier, i.e. "date" or "date-time".</param>
        /// <remarks>
        /// Null values are ignored. Also, <paramref name="delimiter"/> is not url-encoded, special characters should be encoded in advance.
        /// </remarks>
        public void AppendList<T>(string name, IEnumerable<T>? list, bool explode, string delimiter, bool allowReserved, string? format = null)
        {
            if (list is null)
            {
                return;
            }

            if (explode)
            {
                foreach (T item in list)
                {
                    if (item is not null)
                    {
                        Append(name, EscapeValue(item, allowReserved, format));
                    }
                }
            }
            else
            {
                // Append without a value first, leaves a trailing "="
                Append(name, "");

                bool first = true;
                foreach (T item in list)
                {
                    if (item is not null)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            _stringBuilder.Append(delimiter);
                        }

                        _stringBuilder.Append(EscapeValue(item, allowReserved, format));
                    }
                }
            }
        }

        private void Append(string name, string value)
        {
            if (_hasFirstParameter)
            {
                _stringBuilder.Append('&');
            }
            else
            {
                _stringBuilder.Append('?');
                _hasFirstParameter = true;
            }

            _stringBuilder.Append(name);
            _stringBuilder.Append('=');
            _stringBuilder.Append(value);
        }

        private static string EscapeValue<T>(T value, bool allowReserved, string? format) =>
            allowReserved
                ? LiteralSerializer.Instance.Serialize(value, format)
                : Uri.EscapeDataString(LiteralSerializer.Instance.Serialize(value, format));
    }
}

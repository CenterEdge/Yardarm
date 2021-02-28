using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public class LiteralSerializer
    {
        private static readonly MethodInfo _joinListMethod = typeof(LiteralSerializer)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(p => p.IsGenericMethodDefinition && p.Name == nameof(JoinList));

        private static readonly MethodInfo _deserializeListMethod = typeof(LiteralSerializer)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(p => p.IsGenericMethodDefinition && p.Name == nameof(DeserializeList));

        public static LiteralSerializer Instance { get; } = new LiteralSerializer();

        public string Serialize<T>(T value) =>
            value != null
                ? value switch {
                    bool boolean => boolean ? "true" : "false",
                    _ => TypeDescriptor.GetConverter(typeof(T)).ConvertToString(value) ?? ""
                }
                : "";

        [return: NotNullIfNotNull("value")]
        public T Deserialize<T>(string? value) =>
            value != null
                ? (T) TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(value)!
                : default!;

        public string JoinList(string separator, object list, Type itemType)
        {
            MethodInfo joinList = _joinListMethod.MakeGenericMethod(itemType);

            return (string)joinList.Invoke(this, new object[] {separator, list});
        }

        public object DeserializeList(IEnumerable<string> values, Type itemType)
        {
            MethodInfo deserializeList = _deserializeListMethod.MakeGenericMethod(itemType);

            return deserializeList.Invoke(this, new object[] {values});
        }

        // ReSharper disable once UnusedMember.Local
        private string JoinList<T>(string separator, IEnumerable<T> list) =>
            string.Join(separator, list
                .Select(Serialize));

        // ReSharper disable once UnusedMember.Local
        private List<T> DeserializeList<T>(IEnumerable<string> values) =>
            new List<T>(values.Select(Deserialize<T>));
    }
}

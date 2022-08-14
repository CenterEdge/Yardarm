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
        public static LiteralSerializer Instance { get; } = new LiteralSerializer();

        private static readonly MethodInfo s_joinListMethod =
            ((Func<string, IEnumerable<string>, string>)Instance.JoinList<string>).GetMethodInfo().GetGenericMethodDefinition();

        private static readonly MethodInfo s_deserializeListMethod =
            ((Func<IEnumerable<string>, List<string>>)Instance.DeserializeList<string>).GetMethodInfo().GetGenericMethodDefinition();

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
            MethodInfo joinList = s_joinListMethod.MakeGenericMethod(itemType);

            return (string)joinList.Invoke(this, new object[] {separator, list})!;
        }

        public object DeserializeList(IEnumerable<string> values, Type itemType)
        {
            MethodInfo deserializeList = s_deserializeListMethod.MakeGenericMethod(itemType);

            return deserializeList.Invoke(this, new object[] {values})!;
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

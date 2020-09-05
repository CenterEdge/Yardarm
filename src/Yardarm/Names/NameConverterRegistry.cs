using System.Collections.Generic;

namespace Yardarm.Names
{
    public class NameConverterRegistry : Dictionary<string, string>, INameConverterRegistry
    {
        public string Convert(string name) =>
            TryGetValue(name, out string? newName)
                ? newName
                : name;

        public static NameConverterRegistry CreateDefaultRegistry() =>
            new NameConverterRegistry
            {
                { "+1", "PlusOne" },
                { "-1", "MinusOne" },
            };
    }
}

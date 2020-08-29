using System.Collections.Generic;

namespace Yardarm.Names
{
    public interface INameConverterRegistry : IDictionary<string, string>
    {
        string Convert(string name);
    }
}

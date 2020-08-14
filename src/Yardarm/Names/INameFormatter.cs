using System.Diagnostics.CodeAnalysis;

namespace Yardarm.Names
{
    public interface INameFormatter
    {
        [return: NotNullIfNotNull("name")]
        string? Format(string? name);
    }
}

using System;

namespace Yardarm.Packaging
{
    public class ResolvedFrameworkReference
    {
        public string Name { get; }
        public string Path { get; }

        public ResolvedFrameworkReference(string resolvedPair)
        {
            ArgumentNullException.ThrowIfNull(resolvedPair);

            string[] pair = resolvedPair.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2)
            {
                throw new ArgumentException("Invalid framework reference.", nameof(resolvedPair));
            }

            Name = pair[0];
            Path = pair[1];
        }

        public ResolvedFrameworkReference(string name, string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(name);

            Name = name;
            Path = path;
        }
    }
}

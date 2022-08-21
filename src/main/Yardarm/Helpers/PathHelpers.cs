using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yardarm.Helpers
{
    internal static class PathHelpers
    {
        private static readonly HashSet<char> s_invalidPathChars;

        static PathHelpers()
        {
            s_invalidPathChars = Path.GetInvalidPathChars().ToHashSet();

            s_invalidPathChars.Add('{');
            s_invalidPathChars.Add('}');
            s_invalidPathChars.Add(':');
        }

        /// <summary>
        /// Normalize an OpenAPI path into a file path.
        /// </summary>
        /// <param name="path">Path to normalize.</param>
        /// <returns>Normalized path.</returns>
        public static string NormalizePath(string path)
        {
            ArgumentNullException.ThrowIfNull(path);

            var builder = new StringBuilder(path);
            NormalizePath(builder);
            return builder.ToString();
        }

        public static void NormalizePath(StringBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            for (int i = 0; i < builder.Length; i++)
            {
                char ch = builder[i];

                if (Path.DirectorySeparatorChar != '/' && ch == '/')
                {
                    builder[i] = Path.DirectorySeparatorChar;
                }
                else if (s_invalidPathChars.Contains(ch))
                {
                    builder[i] = '_';
                }
            }
        }
    }
}

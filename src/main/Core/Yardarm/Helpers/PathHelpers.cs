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

                if (Path.DirectorySeparatorChar != '/' && ch == Path.DirectorySeparatorChar)
                {
                    builder[i] = '/';
                }
                else if (s_invalidPathChars.Contains(ch))
                {
                    builder[i] = '_';
                }
            }
        }

        /// <summary>
        /// Combine paths in an OS-agnostic manner, always using a "/" separator.
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            ArgumentNullException.ThrowIfNull(path1);
            ArgumentNullException.ThrowIfNull(path2);

            if (path1 == "")
            {
                return path2;
            }

            if (path2 == "")
            {
                return "";
            }

            if (path1[^1] == '/' && path2[0] == '/')
            {
                return $"{path1}{path2[1..]}";
            }

            if (path1[^1] != '/' && path2[0] != '/')
            {
                return $"{path1}/{path2}";
            }

            return $"{path1}{path2}";
        }

        /// <summary>
        /// Combine paths in an OS-agnostic manner, always using a "/" separator.
        /// </summary>
        public static string Combine(string path1, string path2, string path3)
        {
            ArgumentNullException.ThrowIfNull(path1);
            ArgumentNullException.ThrowIfNull(path2);
            ArgumentNullException.ThrowIfNull(path3);

            string temp = Combine(path1, path2);
            return Combine(temp, path3);
        }
    }
}

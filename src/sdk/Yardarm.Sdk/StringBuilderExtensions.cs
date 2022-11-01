using System;
using System.Collections.Generic;
using System.Text;

namespace Yardarm.Build.Tasks
{
    internal static class StringBuilderExtensions
    {
        private static readonly char[] CharsToEscape = {'\\', '"'};

        /// <summary>
        /// Appends wrapped in quotes, escaping any backslashes or double quotes.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="str"></param>
        public static void AppendQuoted(this StringBuilder builder, string? str)
        {
            if (str is null)
            {
                builder.Append("\"\"");
                return;
            }

            builder.EnsureCapacity(builder.Length + str.Length + 2);

            builder.Append('"');

            int searchStart = 0;
            while (searchStart < str.Length)
            {
                int index = str.IndexOfAny(CharsToEscape, searchStart);
                if (index >= searchStart)
                {
                    if (index > searchStart)
                    {
                        builder.Append(str, searchStart, index - searchStart);
                    }

                    builder.Append('\\');
                    builder.Append(str[index]);

                    searchStart = index + 1;
                }
                else
                {
                    builder.Append(str, searchStart, str.Length - searchStart);
                    searchStart = str.Length;
                }
            }

            builder.Append('"');
        }
    }
}

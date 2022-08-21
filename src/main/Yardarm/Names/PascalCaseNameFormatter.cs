using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Yardarm.Names
{
    public class PascalCaseNameFormatter : INameFormatter
    {
        private readonly INameConverterRegistry _nameConverterRegistry;

        public string Prefix { get; }

        public string Suffix { get; }

        public PascalCaseNameFormatter(INameConverterRegistry nameConverterRegistry)
            : this(nameConverterRegistry, "", "")
        {
        }

        public PascalCaseNameFormatter(INameConverterRegistry nameConverterRegistry, string prefix, string suffix)
        {
            _nameConverterRegistry = nameConverterRegistry ?? throw new ArgumentNullException(nameof(nameConverterRegistry));
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Suffix = suffix ?? throw new ArgumentNullException(nameof(suffix));
        }

        [return: NotNullIfNotNull("name")]
        public virtual string? Format(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            name = _nameConverterRegistry.Convert(name!);

            var builder = new StringBuilder(Prefix, Prefix.Length + name.Length + Suffix.Length);

            bool nextCapital = true;
            foreach (char ch in name)
            {
                if (char.IsLetter(ch))
                {
                    if (nextCapital)
                    {
                        builder.Append(char.ToUpperInvariant(ch));
                        nextCapital = false;
                    }
                    else
                    {
                        builder.Append(ch);
                    }
                }
                else if (char.IsDigit(ch))
                {
                    builder.Append(ch);
                    nextCapital = true;
                }
                else
                {
                    nextCapital = true;
                }
            }

            if (Suffix.Length > 0)
            {
                builder.Append(Suffix);
            }

            return builder.ToString();
        }
    }
}

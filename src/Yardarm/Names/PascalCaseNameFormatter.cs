using System;
using System.Text;

namespace Yardarm.Names
{
    public class PascalCaseNameFormatter : INameFormatter
    {
        public static PascalCaseNameFormatter Instance { get; } = new PascalCaseNameFormatter();

        public static PascalCaseNameFormatter InterfacePrefix { get; } = new PascalCaseNameFormatter("I");

        public string Prefix { get; }

        public PascalCaseNameFormatter() : this("")
        {
        }

        public PascalCaseNameFormatter(string prefix)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
        }

        public virtual string Format(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var builder = new StringBuilder(Prefix, Prefix.Length + name.Length);

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

            return builder.ToString();
        }
    }
}

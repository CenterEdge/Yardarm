using System.Text;

namespace Yardarm.Names
{
    public class PascalCaseNameFormatter : INameFormatter
    {
        public static PascalCaseNameFormatter Instance { get; } = new PascalCaseNameFormatter();

        public virtual string Format(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var builder = new StringBuilder(name.Length);

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

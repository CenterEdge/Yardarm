namespace Yardarm.Names
{
    public class CamelCaseNameFormatter : INameFormatter
    {
        public static CamelCaseNameFormatter Instance { get; } = new CamelCaseNameFormatter();

        public virtual string Format(string name) => throw new System.NotImplementedException();
    }
}

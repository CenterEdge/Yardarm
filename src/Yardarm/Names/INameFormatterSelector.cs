namespace Yardarm.Names
{
    public interface INameFormatterSelector
    {
        public INameFormatter GetFormatter(NameKind nameKind);
    }
}

using System;

namespace Yardarm.Names.Internal
{
    public class NameFormatterSelector : INameFormatterSelector
    {
        private readonly PascalCaseNameFormatter _default;
        private readonly PascalCaseNameFormatter _interfacePrefix;
        private readonly PascalCaseNameFormatter _asyncSuffix;

        public NameFormatterSelector(INameConverterRegistry nameConverterRegistry)
        {
            if (nameConverterRegistry == null)
            {
                throw new ArgumentNullException(nameof(nameConverterRegistry));
            }

            _default = new PascalCaseNameFormatter(nameConverterRegistry);
            _interfacePrefix = new PascalCaseNameFormatter(nameConverterRegistry, "I", "");
            _asyncSuffix = new PascalCaseNameFormatter(nameConverterRegistry, "", "Async");
        }

        public INameFormatter GetFormatter(NameKind nameKind) => nameKind switch
        {
            NameKind.Interface => _interfacePrefix,
            NameKind.AsyncMethod => _asyncSuffix,
            _ => _default
        };
    }
}

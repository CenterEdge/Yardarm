using System;

namespace Yardarm.Packaging
{
    public static class NuGetFrameworkConstants
    {
        public const string NetStandardFramework = ".NETStandard";
        public const string NetCoreApp = ".NETCoreApp";

        public static readonly Version NetStandard20 = new(2, 0, 0, 0);
        public static readonly Version NetStandard21 = new(2, 1, 0, 0);
    }
}

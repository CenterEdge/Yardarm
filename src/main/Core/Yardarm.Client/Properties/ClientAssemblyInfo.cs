#if FORTESTS
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Yardarm.Benchmarks")]
[assembly: InternalsVisibleTo("Yardarm.Client.UnitTests")]
[assembly: InternalsVisibleTo("Yardarm.MicrosoftExtensionsHttp.Client")]
[assembly: InternalsVisibleTo("Yardarm.NewtonsoftJson.Client")]
[assembly: InternalsVisibleTo("Yardarm.NewtonsoftJson.Client.UnitTests")]
[assembly: InternalsVisibleTo("Yardarm.NodaTime.Client")]
[assembly: InternalsVisibleTo("Yardarm.NodaTime.Client.UnitTests")]
[assembly: InternalsVisibleTo("Yardarm.SystemTextJson.Client")]
[assembly: InternalsVisibleTo("Yardarm.SystemTextJson.Client.UnitTests")]
#endif

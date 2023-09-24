#if FORTESTS
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Yardarm.Client.UnitTests")]
[assembly: InternalsVisibleTo("Yardarm.MicrosoftExtensionsHttp.Client")]
[assembly: InternalsVisibleTo("Yardarm.NewtonsoftJson.Client")]
[assembly: InternalsVisibleTo("Yardarm.SystemTextJson.Client")]
#endif

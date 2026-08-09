using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LoggerBase = NuGet.Common.LoggerBase;
using ILogMessage = NuGet.Common.ILogMessage;

namespace Yardarm.Generation.Internal
{
    public class NuGetLogger : LoggerBase
    {
        private readonly ILogger _logger;

        public NuGetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public override void Log(ILogMessage message)
        {
            _logger.Log(ConvertLogLevel(message.Level), new EventId((int) message.Code), message.Message);
        }

        public override Task LogAsync(ILogMessage message)
        {
            Log(message);
            return Task.CompletedTask;
        }

        private static LogLevel ConvertLogLevel(NuGet.Common.LogLevel level) =>
            level switch
            {
                NuGet.Common.LogLevel.Debug => LogLevel.Debug,
                NuGet.Common.LogLevel.Verbose => LogLevel.Information,
                NuGet.Common.LogLevel.Information => LogLevel.Information,
                NuGet.Common.LogLevel.Minimal => LogLevel.Information,
                NuGet.Common.LogLevel.Warning => LogLevel.Warning,
                NuGet.Common.LogLevel.Error => LogLevel.Error,
                _ => LogLevel.Debug
            };
    }
}

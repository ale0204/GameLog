using GameForum.Application.Common.Logging;
using Serilog;

namespace GameForum.Infrastructure.Core.Logging;

public class SerilogAppLogger : IAppLogger
{
    private readonly ILogger _logger;

    public SerilogAppLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void Information(string message) => _logger.Information(message);

    public void Warning(string message) => _logger.Warning(message);

    public void Error(string message) => _logger.Error(message);
}

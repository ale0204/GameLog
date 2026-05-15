using GameForum.Application.Common.ExternalServices;
using GameForum.Application.Common.Logging;
using GameForum.Infrastructure.Core.ExternalServices;
using GameForum.Infrastructure.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameForum.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Logging — Serilog static Log.Logger is configured in Program.cs (Stage 6).
        // We wrap it with IAppLogger here so Application code never sees Serilog directly.
        services.AddSingleton<IAppLogger, SerilogAppLogger>(sp =>
            new SerilogAppLogger(Serilog.Log.Logger));

        // RAWG.io HTTP client — base URL from configuration.
        var baseUrl = configuration["RAWG:BaseUrl"]
            ?? throw new InvalidOperationException("RAWG:BaseUrl is not configured.");
        services.AddHttpClient<IRawgClient, RawgClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // TrendingScoreWorker intentionally NOT registered — post-MVP.
        // services.AddHostedService<TrendingScoreWorker>();

        return services;
    }
}

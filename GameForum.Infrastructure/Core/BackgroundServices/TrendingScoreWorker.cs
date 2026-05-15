using GameForum.Application.Common.Logging;
using Microsoft.Extensions.Hosting;

namespace GameForum.Infrastructure.Core.BackgroundServices;

// Skeleton: not registered in DI. Will be wired up post-MVP when the
// scoring algorithm is implemented. For now Threads and Posts have a
// TrendingScore column that stays at 0 and sorting falls back to
// CreatedAt / LastActivityAt.
public class TrendingScoreWorker : BackgroundService
{
    private readonly IAppLogger _logger;
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(15);

    public TrendingScoreWorker(IAppLogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("TrendingScoreWorker started (skeleton — no scoring implemented).");
        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO: implement scoring algorithm
            // - Load active threads / posts from DB (Repository or AppDbContext via IServiceScope)
            // - Compute trending score (e.g., votes weighted by age decay)
            // - Update TrendingScore field
            // - SaveChangesAsync
            try
            {
                await Task.Delay(TickInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // expected on shutdown
            }
        }
        _logger.Information("TrendingScoreWorker stopped.");
    }
}

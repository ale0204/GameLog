using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameForum.DataAccess.Core.EfCore;

/// Used by `dotnet ef` CLI to construct AppDbContext at design time
/// (when generating or applying migrations). Reads the connection string
/// from the GAMEFORUM_CONNECTION_STRING environment variable so passwords
/// never end up in source control.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("GAMEFORUM_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "GAMEFORUM_CONNECTION_STRING environment variable is not set. " +
                "Set it before running EF tooling, for example: " +
                "$env:GAMEFORUM_CONNECTION_STRING = \"Host=localhost;Port=5432;Database=gameforum;Username=postgres;Password=...\"");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}

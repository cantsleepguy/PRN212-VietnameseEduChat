using Microsoft.Extensions.Diagnostics.HealthChecks;
using PRN212_VietnameseEduChat.DataAccess.Context;

namespace PRN212_VietnameseEduChat.Health;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _database;

    public DatabaseHealthCheck(ApplicationDbContext database)
    {
        _database = database;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _database.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Database is reachable.")
                : HealthCheckResult.Unhealthy("Database is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Database readiness check failed.",
                exception);
        }
    }
}

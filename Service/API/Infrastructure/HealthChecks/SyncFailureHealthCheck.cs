using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Infrastructure.HealthChecks;

public sealed class SyncFailureHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SyncFailureHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repo = scope.ServiceProvider
            .GetRequiredService<IPackageEventHouseholdRepository>();

        // Look at last 30 minutes (tune as needed)
        var failedCount = await repo.CountRecentFailedSynchronisationsAsync(
            TimeSpan.FromMinutes(30),
            cancellationToken);

        if (failedCount == 0)
        {
            return HealthCheckResult.Healthy("No recent failed synchronisations");
        }

        if (failedCount < 10)
        {
            return HealthCheckResult.Degraded(
                $"{failedCount} recent failed synchronisations",
                data: new Dictionary<string, object>
                {
                    ["failedCount"] = failedCount
                });
        }

        return HealthCheckResult.Unhealthy(
            $"{failedCount} recent failed synchronisations",
            data: new Dictionary<string, object>
            {
                ["failedCount"] = failedCount
            });
    }
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Infrastructure.HealthChecks;

public sealed class SyncProcessingHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SyncProcessingHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPackageEventHouseholdRepository>();

        var stuck = await repo.CountStuckSynchronisationsAsync(TimeSpan.FromMinutes(10), cancellationToken);

        return stuck == 0
            ? HealthCheckResult.Healthy("No stuck synchronisations")
            : HealthCheckResult.Degraded($"{stuck} stuck synchronisations");
    }
}
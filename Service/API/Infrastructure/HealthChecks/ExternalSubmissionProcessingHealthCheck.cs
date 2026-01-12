using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;

namespace Staging.API.Infrastructure.HealthChecks;

public sealed class ExternalSubmissionProcessingHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExternalSubmissionProcessingHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IExternalSubmissionRepository>();

        var stuck = await repo.CountStuckSubmissionsAsync(TimeSpan.FromMinutes(10), cancellationToken);

        return stuck == 0
            ? HealthCheckResult.Healthy("No stuck external submissions")
            : HealthCheckResult.Degraded($"{stuck} stuck external submissions");
    }
}
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Staging.API.Infrastructure.HealthChecks;

public sealed class SyncWorkerHealthCheck : IHealthCheck
{
    private readonly SyncWorkerMonitor _monitor;

    public SyncWorkerHealthCheck(SyncWorkerMonitor monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _monitor.ActiveWorkers > 0
                ? HealthCheckResult.Healthy($"{_monitor.ActiveWorkers} workers running")
                : HealthCheckResult.Unhealthy("No active workers"));
    }
}

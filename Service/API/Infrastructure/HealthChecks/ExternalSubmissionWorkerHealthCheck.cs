using Microsoft.Extensions.Diagnostics.HealthChecks;
using Staging.API.Infrastructure.BackgroundProcessing.ExternalSubmission;

namespace Staging.API.Infrastructure.HealthChecks;

public sealed class ExternalSubmissionWorkerHealthCheck : IHealthCheck
{
    private readonly ExternalSubmissionWorkerMonitor _monitor;

    public ExternalSubmissionWorkerHealthCheck(ExternalSubmissionWorkerMonitor monitor)
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

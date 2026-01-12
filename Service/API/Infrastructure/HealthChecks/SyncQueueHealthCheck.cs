using Microsoft.Extensions.Diagnostics.HealthChecks;
using Staging.API.Infrastructure.BackgroundProcessing;
using Staging.API.Infrastructure.BackgroundProcessing.Sync;

namespace Staging.API.Infrastructure.HealthChecks;

public sealed class SyncQueueHealthCheck : IHealthCheck
{
    private readonly IWorkQueue<SyncWorkItem> _queue;

    public SyncQueueHealthCheck(IWorkQueue<SyncWorkItem> queue)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        if (_queue.Capacity <= 0) return Task.FromResult(
            HealthCheckResult.Healthy("Queue capacity not set")); 

        var ratio = (double)_queue.ApproximateCount / _queue.Capacity;

        if (ratio > 0.9)
            return Task.FromResult(HealthCheckResult.Unhealthy("Queue almost full"));

        if (ratio > 0.7)
            return Task.FromResult(HealthCheckResult.Degraded("Queue growing"));

        return Task.FromResult(
            HealthCheckResult.Healthy("Queue OK"));
    }
}

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Staging.API.Infrastructure.BackgroundProcessing;
using Staging.API.Infrastructure.BackgroundProcessing.ExternalSubmission;
using System.Threading;

namespace Staging.API.Infrastructure.HealthChecks;

public sealed class ExternalSubmissionQueueHealthCheck : IHealthCheck
{
    private readonly IWorkQueue<ExternalSubmissionWorkItem> _queue;

    public ExternalSubmissionQueueHealthCheck(IWorkQueue<ExternalSubmissionWorkItem> queue)
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

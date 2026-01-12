namespace Staging.API.Infrastructure.BackgroundProcessing.ExternalSubmission;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Staging.API.Application.Services.ExternalSubmissions;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;

public sealed class ExternalSubmissionWorkerProcessor : IWorkerProcessor<ExternalSubmissionWorkItem>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<ExternalSubmissionWorkerOptions> _options;
    private readonly ILogger<ExternalSubmissionWorkerProcessor> _logger;

    public ExternalSubmissionWorkerProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<ExternalSubmissionWorkerOptions> options,
        ILogger<ExternalSubmissionWorkerProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task SeedAsync(IWorkQueue<ExternalSubmissionWorkItem> queue, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IExternalSubmissionRepository>();

        _logger.LogInformation("Resetting processing external submissions to pending at startup...");
        await repo.ResetProcessingToPendingAsync(cancellationToken);

        var retryAfter = TimeSpan.FromMinutes(_options.Value.RetryAfterMinutes);

        var ids = await repo.GetPendingSubmissionIdsAsync(
            batchSize: _options.Value.StartupBatchSize,
            maxAttempts: _options.Value.MaxAttempts,
            retryAfter: retryAfter,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Requeueing {Count} pending external submission aggregates at startup", ids.Count);

        foreach (var submissionId in ids)
            await queue.EnqueueAsync(new ExternalSubmissionWorkItem(submissionId), cancellationToken);
    }

    public async Task ProcessAsync(ExternalSubmissionWorkItem item, int workerId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IExternalSubmissionRepository>();
        var handlers = scope.ServiceProvider
            .GetRequiredService<IEnumerable<IExternalSubmissionHandler>>()
            .ToDictionary(h => h.SubmissionType);

        var aggregate = await repo.GetBySubmissionIdAsync(item.SubmissionId, cancellationToken);
        if (aggregate == null)
        {
            _logger.LogWarning("Worker {WorkerId}: Aggregate not found for SubmissionId={SubmissionId}",
                workerId, item.SubmissionId);
            return;
        }

        var submission = aggregate.Submissions.SingleOrDefault(s => s.Id == item.SubmissionId);
        if (submission == null)
        {
            _logger.LogWarning("Worker {WorkerId}: Submission not found on aggregate. SubmissionId={SubmissionId}",
                workerId, item.SubmissionId);
            return;
        }

        if (!submission.CanRetry(_options.Value.MaxAttempts))
            return;

        if (submission.LastAttemptAt.HasValue &&
            DateTime.UtcNow - submission.LastAttemptAt.Value < _options.Value.RetryAfter)
            return;

        if (!submission.TrySetToProcessing())
        {
            _logger.LogWarning("Worker {WorkerId}: Try set to processing returned false. SubmissionId={SubmissionId}",
                workerId, item.SubmissionId);
            return;
        }

        await repo.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        if (!handlers.TryGetValue(submission.Type, out var handler))
        {
            aggregate.MarkFailure(submission, "No handler registered", null);
            repo.Update(aggregate);
            await repo.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return;
        }

        var result = await handler.HandleAsync(submission, cancellationToken);

        if (result.Success)
            aggregate.MarkSuccess(submission, result.ExternalId, result.ResponsePayload);
        else
            aggregate.MarkFailure(submission, "External submission failed", result.ErrorMessage);

        repo.Update(aggregate);
        await repo.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }
}

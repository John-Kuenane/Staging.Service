namespace Staging.API.Infrastructure.BackgroundProcessing.Sync;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Staging.Domain.AggregatesModel.PackageAggregate;
using Staging.Domain.Factories;
using Staging.Domain.ProcessorActivities;
using Staging.Domain.SeedWork;
using System.Threading;

public sealed class SyncWorkerProcessor : IWorkerProcessor<SyncWorkItem>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<SyncWorkerOptions> _options;
    private readonly ILogger<SyncWorkerProcessor> _logger;

    public SyncWorkerProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncWorkerOptions> options,
        ILogger<SyncWorkerProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task SeedAsync(IWorkQueue<SyncWorkItem> queue, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPackageEventHouseholdRepository>();

        _logger.LogInformation("Resetting processing synchronisations to pending at startup...");
        await repo.ResetProcessingToPendingAsync(cancellationToken);

        var ids = await repo.GetPendingSynchronisationIdsAsync(
            _options.Value.StartupBatchSize, cancellationToken);

        _logger.LogInformation("Requeueing {Count} pending synchronisations at startup", ids.Count);

        foreach (var id in ids)
            await queue.EnqueueAsync(new SyncWorkItem(id), cancellationToken);
    }

    public async Task ProcessAsync(SyncWorkItem item, int workerId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repo = scope.ServiceProvider.GetRequiredService<IPackageEventHouseholdRepository>();
        var processorFactory = scope.ServiceProvider.GetRequiredService<IPayloadProcessorFactory>();

        var household = await repo.GetBySynchronisationIdAsync(item.SynchronisationId);
        if (household == null)
        {
            _logger.LogWarning("Worker {WorkerId}: Household not found for SyncId={Id}", workerId, item.SynchronisationId);
            return;
        }

        var synch = household.Synchs.SingleOrDefault(s => s.Id == item.SynchronisationId);
        if (synch == null)
        {
            _logger.LogWarning("Worker {WorkerId}: Synch not found for SyncId={Id}", workerId, item.SynchronisationId);
            return;
        }

        if (synch.IsFinalised)
        {
            _logger.LogDebug("Worker {WorkerId}: SyncWorkItem {Id} already finalised", workerId, item.SynchronisationId);
            return;
        }

        if (!synch.TrySetToProcessing())
        {
            _logger.LogDebug("Worker {WorkerId}: SyncWorkItem {Id} could not be set to processing", workerId, item.SynchronisationId);
            return;
        }

        await repo.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        _logger.LogInformation("Worker {WorkerId} started processing SyncWorkItem {Id}", workerId, item.SynchronisationId);

        var activity = PrepareActivity(household, synch);
        var result = await processorFactory.ProcessAsync(activity, cancellationToken);

        if (result.Successful)
        {
            synch.SetToProcessed();
            _logger.LogInformation("Worker {WorkerId} successfully processed SyncWorkItem {Id}", workerId, item.SynchronisationId);
        }
        else
        {
            synch.SetToFailed(result.ResultMessage);
            _logger.LogWarning("Worker {WorkerId} failed processing SyncWorkItem {Id}: {Message}", workerId, item.SynchronisationId, result.ResultMessage);
        }

        await repo.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    private static Processor PrepareActivity(PackageEventHousehold household, PackageEventHouseholdSynch synch)
    {
        if (synch.Payload.Contains("\"newCommunityClassification\":") ||
            synch.Payload.Contains("\"newHouseholdClassification\":"))
            return new DataListingEventPayloadActivity(household, synch);

        if (synch.Payload.Contains("\"householdAttributes\":["))
            return new DataCollectionEventPayloadActivity(household, synch);

        throw new NotImplementedException();
    }
}


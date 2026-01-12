using Staging.API.Infrastructure.BackgroundProcessing;
using Staging.API.Infrastructure.BackgroundProcessing.Sync;
using Staging.Domain.Events;

namespace Staging.API.Application.DomainEventHandlers;

public class EnqueueWhenPayloadSynchronisedDomainEventHandler
    : INotificationHandler<PayloadSynchronisedDomainEvent>  
{
    private readonly IWorkQueue<SyncWorkItem> _queue;
    private readonly ILogger<EnqueueWhenPayloadSynchronisedDomainEventHandler> _logger;

    public EnqueueWhenPayloadSynchronisedDomainEventHandler(
        IWorkQueue<SyncWorkItem> queue,
        ILogger<EnqueueWhenPayloadSynchronisedDomainEventHandler> logger)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PayloadSynchronisedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Enqueueing PayloadSynchronisedDomainEvent for HouseholdSynchId {Id}",
                domainEvent.PackageEventHouseholdSynch.Id);

            await _queue.EnqueueAsync(
                new SyncWorkItem(domainEvent.PackageEventHouseholdSynch.Id),
                cancellationToken);

            _logger.LogInformation("Enqueued PayloadSynchronisedDomainEvent for HouseholdSynchId {Id}",
                domainEvent.PackageEventHouseholdSynch.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to enqueue PayloadSynchronisedDomainEvent for HouseholdSynchId {Id}",
                domainEvent.PackageEventHouseholdSynch.Id);
        }
    }
}
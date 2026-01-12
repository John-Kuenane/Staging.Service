using Staging.API.Application.Common.Idempotency;
using Staging.API.Application.Mapper;
using Staging.API.Application.Services.ExternalSubmissions;
using Staging.API.Infrastructure.BackgroundProcessing;
using Staging.API.Infrastructure.BackgroundProcessing.ExternalSubmission;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;
using Staging.Domain.Events;
using System.Text.Json;

namespace Staging.API.Application.DomainEventHandlers;

public class SubmitDataFlagWhenAddedDomainEventHandler
    : INotificationHandler<DataFlagAddedDomainEvent>
{
    private readonly IExternalSubmissionRepository _repository;
    private readonly IVendorPayloadMapper _vendorPayloadMapper;
    private readonly IWorkQueue<ExternalSubmissionWorkItem> _queue;
    private readonly ILogger<SubmitDataFlagWhenAddedDomainEventHandler> _logger;

    private const string Vendor = "Freshdesk";

    public SubmitDataFlagWhenAddedDomainEventHandler(
        IExternalSubmissionRepository repository,
        IVendorPayloadMapper vendorPayloadMapper,
        IWorkQueue<ExternalSubmissionWorkItem> queue,
        ILogger<SubmitDataFlagWhenAddedDomainEventHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _vendorPayloadMapper = vendorPayloadMapper ?? throw new ArgumentNullException(nameof(vendorPayloadMapper));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DataFlagAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            if(!String.Equals(domainEvent.Vendor, "tdl"))
            {
                _logger.LogInformation(
                    "Skipping DataFlagAddedDomainEvent for PackageEventDataFlagId={Id} as Vendor is not 'tdl'",
                    domainEvent.PackageEventDataFlag.Id);
                return;
            }   

            _logger.LogInformation(
            "Handling DataFlagAddedDomainEvent for PackageEventDataFlagId={Id}",
            domainEvent.PackageEventDataFlag.Id);

            var requestKey = RequestKeyFactory.ForDataFlagFreshdesk(domainEvent);

            var sourceType = "DataFlag";
            var sourceId = domainEvent.PackageEventDataFlag.Id;

            var exists = await _repository.SubmissionExistsAsync(
                sourceType: sourceType,
                sourceId: sourceId,
                type: ExternalSubmissionType.FreshdeskDataFlag,
                vendor: Vendor,
                requestKey: requestKey,
                cancellationToken: cancellationToken);

            if (exists)
            {
                _logger.LogInformation(
                    "Skipping DataFlag enqueue (already exists). PackageEventId={PackageEventId}, HouseholdId={HouseholdId}, RequestKey={RequestKey}",
                    domainEvent.PackageEvent.Id,
                    domainEvent.PackageEventDataFlag.PackageEventHouseholdId,
                    requestKey);
                return;
            }

            var aggregate = new ExternalSubmissionAggregate(
                sourceType: sourceType,
                sourceId: sourceId);

            _logger.LogDebug(
                 "Mapping payload for vendor {Vendor} and PackageEventDataFlagId={Id}",
                 Vendor,
                 domainEvent.PackageEventDataFlag.Id);

            var mappedPayload = _vendorPayloadMapper.Map(
                "FreshDesk",
                domainEvent.PackageEventDataFlag);

            var submission = aggregate.AddSubmission(
                ExternalSubmissionType.FreshdeskDataFlag,
                Vendor,
                JsonSerializer.Serialize(mappedPayload),
                requestKey: requestKey
            );

            _repository.Add(aggregate);
            await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            // Enqueue the aggregate ID for processing
            await _queue.EnqueueAsync(
                new ExternalSubmissionWorkItem(submission.Id),
                cancellationToken);

            _logger.LogInformation(
                "Enqueued ExternalSubmissionAggregateId={AggregateId} for vendor {Vendor}. RequestKey={RequestKey}",
                aggregate.Id,
                Vendor,
                requestKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling DataFlagAddedDomainEvent for PackageEventDataFlagId={Id}",
                domainEvent.PackageEventDataFlag.Id);
            throw;
        }
    }
}
using MediatR;
using Microsoft.Extensions.Options;
using Staging.API.Application.Common.Idempotency;
using Staging.API.Application.Services.ExternalSubmissions.IdentityVerification;
using Staging.API.Infrastructure.BackgroundProcessing;
using Staging.API.Infrastructure.BackgroundProcessing.ExternalSubmission;
using Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;
using Staging.Domain.Events;
using System.Text.Json;

namespace Staging.API.Application.DomainEventHandlers.HouseholdMemberIdentityMaterialised;

public sealed class SubmitIdentityVerificationWhenMaterialisedDomainEventHandler
    : INotificationHandler<HouseholdMemberIdentityMaterialisedDomainEvent>
{
    private const string FlowName = "IdentityVerification";

    private readonly IExternalSubmissionRepository _repository;
    private readonly IWorkQueue<ExternalSubmissionWorkItem> _queue;
    private readonly IOptions<IdentityVerificationSettings> _settings;
    private readonly ILogger<SubmitIdentityVerificationWhenMaterialisedDomainEventHandler> _logger;

    public SubmitIdentityVerificationWhenMaterialisedDomainEventHandler(
        IExternalSubmissionRepository repository,
        IWorkQueue<ExternalSubmissionWorkItem> queue,
        IOptions<IdentityVerificationSettings> settings,
        ILogger<SubmitIdentityVerificationWhenMaterialisedDomainEventHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(HouseholdMemberIdentityMaterialisedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var member = domainEvent.PackageEventHouseholdMember;

        if (member is null)
        {
            _logger.LogWarning("{Flow}: Domain event received without member loaded.", FlowName);
            return;
        }

        var provider = _settings.Value.Provider; // "NICR" or "Golsabs"
        var requestKey = RequestKeyFactory.ForIdentityVerification(domainEvent);

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Flow"] = FlowName,
            ["MemberId"] = member.Id,
            ["Provider"] = provider,
            ["RequestKey"] = requestKey,
            ["SourceType"] = "IdentityVerification",
            ["SourceId"] = member.Id
        });

        _logger.LogInformation("{Flow}: Identity materialised event received.", FlowName);

        var identity = member.IdentityForVerification;

        if (!identity.IsCompleteForVerification())
        {
            _logger.LogInformation("{Flow}: Skipping enqueue - identity incomplete.", FlowName);
            return;
        }

        _logger.LogDebug(
            "{Flow}: Identity eligible for verification. HasNames={HasNames} HasDob={HasDob} HasIdNumber={HasIdNumber}",
            FlowName,
            !string.IsNullOrWhiteSpace(identity.FirstName) && !string.IsNullOrWhiteSpace(identity.Surname),
            identity.DateOfBirth.HasValue,
            !string.IsNullOrWhiteSpace(identity.IdentificationNumber));

        var exists = await _repository.SubmissionExistsAsync(
            sourceType: "IdentityVerification",
            sourceId: member.Id,
            type: ExternalSubmissionType.IdentityVerification,
            vendor: provider,
            requestKey: requestKey,
            cancellationToken: cancellationToken);

        if (exists)
        {
            _logger.LogInformation("{Flow}: Idempotent skip - submission already exists.", FlowName);
            return;
        }

        _logger.LogInformation("{Flow}: Creating external submission + enqueueing.", FlowName);

        var aggregate = new ExternalSubmissionAggregate(
            sourceType: "IdentityVerification",
            sourceId: member.Id);

        var payload = new IdentityVerificationSubmissionPayload(
            MemberId: domainEvent.PackageEventHouseholdMember.Id,
            provider,
            Identity: identity
        );

        var submission = aggregate.AddSubmission(
            type: ExternalSubmissionType.IdentityVerification,
            vendor: provider,
            payload: JsonSerializer.Serialize(payload),
            requestKey: requestKey
        );

        _repository.Add(aggregate);

        try
        {
            await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            _logger.LogInformation("{Flow}: Submission persisted. SubmissionId={SubmissionId}", FlowName, submission.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Flow}: Failed persisting submission before enqueue.", FlowName);
            throw;
        }

        try
        {
            await _queue.EnqueueAsync(new ExternalSubmissionWorkItem(submission.Id), cancellationToken);

            _logger.LogInformation(
                "{Flow}: Enqueued work item. SubmissionId={SubmissionId} Status={Status}",
                FlowName,
                submission.Id,
                submission.Status);
        }
        catch (Exception ex)
        {
            // At this point the submission exists in DB but wasn't queued.
            // Startup seeding/retry logic should eventually pick it up.
            _logger.LogError(
                ex,
                "{Flow}: Failed to enqueue work item for persisted submission. SubmissionId={SubmissionId}. It should be picked up by seeding/retry.",
                FlowName,
                submission.Id);

            throw;
        }
    }
}

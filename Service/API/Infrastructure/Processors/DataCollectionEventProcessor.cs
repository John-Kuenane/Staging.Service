using Newtonsoft.Json;
using Staging.Domain.AggregatesModel.PackageAggregate;
using Staging.Domain.Exceptions;
using Staging.Domain.Models;
using Staging.Domain.ProcessorActivities;
using Staging.Domain.SeedWork;
using Staging.Domain.Services;

namespace Staging.API.Infrastructure.Processors;


public class DataCollectionEventProcessor
    : PayloadProcessor<DataCollectionEventPayloadActivity>
{
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<DataCollectionEventProcessor> _logger;

    public DataCollectionEventProcessor(
        IPackageRepository packageRepository,
        ILogger<DataCollectionEventProcessor> logger)
    {
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<ExecutionResult> ExecuteCoreAsync(DataCollectionEventPayloadActivity activity, CancellationToken cancellationToken)
    {
        if (activity.PackageEventHousehold == null)
        {
            activity.MarkAsBlocked("No household to process");
            return ExecutionResult.Failed(activity.ActivityType, "No household to process.");
        }

        if (activity.PackageEventHouseholdSynch == null)
        {
            activity.MarkAsBlocked("No household synchronisation to process");
            return ExecutionResult.Failed(activity.ActivityType, "No household synchronisation to process.");
        }

        if (String.IsNullOrWhiteSpace(activity.PackageEventHouseholdSynch.Payload))
        {
            activity.MarkAsBlocked("No payload to process");
            return ExecutionResult.Failed(activity.ActivityType, "No payload to process.");
        }

        activity.MarkAsInProgress();

        var deserialisedPayload = JsonConvert.DeserializeObject<DataCollectionEventDetail>(activity.PackageEventHouseholdSynch.Payload);
        if (deserialisedPayload == null)
        {
            activity.MarkAsBlocked("Unable to deserialise payload");
            return ExecutionResult.Failed(activity.ActivityType, "Unable to deserialise payload.");
        }

        try
        {
            ProcessHouseholdChanges(activity.PackageEventHousehold, deserialisedPayload.HouseholdAttributes);
            ProcessMemberStatusChange(activity.PackageEventHousehold, deserialisedPayload);
            ProcessHouseholdMemberChanges(activity.PackageEventHousehold, deserialisedPayload);
            ProcessNewMembers(activity.PackageEventHousehold, deserialisedPayload);

            activity.PackageEventHousehold.SetCollectionToEnumerated();

            await _packageRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
            catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DataCollectionEvent for HouseholdId {HouseholdId}", activity.PackageEventHousehold.Id);

            activity.MarkAsBlocked(ex.Message);

            return ExecutionResult.Failed(activity.ActivityType, $"{ex}: processing payload with ID: {activity.PackageEventHousehold.Id}");
        }

        activity.MarkAsSuccessfullyCompleted();

        return ExecutionResult.Success(activity.ActivityType, null);
    }

    private void ProcessHouseholdChanges(PackageEventHousehold packageEventHousehold, List<DataCollectionEventDetailAttribute> changeAttributes)
    {
        var changeAttributeList = changeAttributes.Where(ha => ha.ValueState == "Dirty");

        foreach (var changeAttribute in changeAttributeList)
        {
            packageEventHousehold.SetAttributeValue(changeAttribute.Key, changeAttribute.NewValue);
        }
    }

    private void ProcessMemberStatusChange(PackageEventHousehold packageEventHousehold, DataCollectionEventDetail deserialisedPayload)
    {
        var membersToRemove = deserialisedPayload.Members.Where(hm => hm.StatusDetails?.Status == "Removed");

        foreach (var memberToRemove in membersToRemove)
        {
            var currentHouseholdMember = ResolveExistingMember(packageEventHousehold, memberToRemove);

            if (currentHouseholdMember == null)
            {
                throw new KeyNotFoundException(
                    $"Unable to locate household member for marking as not current. " +
                    $"HouseholdMemberId: {memberToRemove.HouseholdMemberId}, " +
                    $"HouseholdMemberGuid: {memberToRemove.HouseholdMemberGuid}");
            }

            currentHouseholdMember.SetToRemoved(memberToRemove.StatusDetails.RemovedReason);
        }
    }

    private void ProcessHouseholdMemberChanges(PackageEventHousehold packageEventHousehold, DataCollectionEventDetail deserialisedPayload)
    {
        foreach (var memberToUpdate in (deserialisedPayload.Members ?? new List<DataCollectionEventMember>())
            .Where(hm => hm.HouseholdMemberGuid != "NEWGUID"
                      && hm.StatusDetails?.Status != "Removed"))
        {
            var currentHouseholdMember = ResolveExistingMember(packageEventHousehold, memberToUpdate);

            if (currentHouseholdMember == null)
            {
                throw new KeyNotFoundException(
                    $"Unable to locate household member for update. " +
                    $"HouseholdMemberId: {memberToUpdate.HouseholdMemberId}, " +
                    $"HouseholdMemberGuid: {memberToUpdate.HouseholdMemberGuid}");
            }

            var changeAttributeList = (memberToUpdate.HouseholdMemberAttributes ?? new List<DataCollectionEventDetailAttribute>())
                .Where(ha => ha.ValueState == "Dirty");
            foreach (var changeAttribute in changeAttributeList)
            {
                currentHouseholdMember.SetAttributeValue(changeAttribute.Key, changeAttribute.NewValue);
            }

            currentHouseholdMember.MaterialiseEnumeratedIdentity();
        }
    }

    private void ProcessNewMembers(PackageEventHousehold packageEventHousehold, DataCollectionEventDetail deserialisedPayload)
    {
        foreach (var memberToAdd in deserialisedPayload.Members.Where(hm => hm.HouseholdMemberGuid == "NEWGUID"))
        {
            GuardNewMemberPayload(memberToAdd);
            var newHouseholdMember = packageEventHousehold.AddNewMember(memberToAdd);

            var changeAttributeList = memberToAdd.HouseholdMemberAttributes.Where(ha => ha.ValueState == "Dirty");
            foreach (var changeAttribute in changeAttributeList)
            {
                newHouseholdMember.SetAttributeValue(changeAttribute.Key, changeAttribute.NewValue);
            }

            newHouseholdMember.MaterialiseEnumeratedIdentity();
        }
    }

    private void GuardNewMemberPayload(DataCollectionEventMember deserialisedMemberPayload)
    {
        if (String.IsNullOrWhiteSpace(deserialisedMemberPayload.Name))
        {
            throw new DomainException($"Name not specified for new member");
        }

        if (String.IsNullOrWhiteSpace(deserialisedMemberPayload.Surname))
        {
            throw new DomainException($"Surname not specified for new member");
        }

        //var validGenders = new List<string>() { "Male", "Female" };
        //if (!validGenders.Contains(deserialisedMemberPayload.Gender))
        //{
        //    throw new DomainException($"Gender not specified or invalid for new member");
        //}

        //if (String.IsNullOrWhiteSpace(deserialisedMemberPayload.DateOfBirth))
        //{
        //    throw new DomainException($"Date of birth not specified for new member");
        //}
    }

    // Historic remediation support:
    // Some previously synchronised members contain Guid.Empty due to legacy data issues.
    // In these cases, HouseholdMemberId is used as the primary identifier instead of GUID.
    private PackageEventHouseholdMember ResolveExistingMember(
        PackageEventHousehold packageEventHousehold,
        DataCollectionEventMember member)
    {
        var householdMemberId = member.HouseholdMemberId;

        if (householdMemberId <= 0)
        {
            throw new InvalidOperationException(
                $"Invalid HouseholdMemberId '{householdMemberId}' in sync payload.");
        }

        var hasParsedGuid = Guid.TryParse(member.HouseholdMemberGuid, out var householdMemberGuid);
        var hasUsableGuid = hasParsedGuid && householdMemberGuid != Guid.Empty;

        var byId = packageEventHousehold.Members
            .SingleOrDefault(hm => hm.HouseholdMemberId == householdMemberId);

        if (hasUsableGuid)
        {
            var byGuid = packageEventHousehold.Members
                .SingleOrDefault(hm => hm.HouseholdMemberGuid == householdMemberGuid);

            if (byGuid != null && byId != null && byGuid.HouseholdMemberId != byId.HouseholdMemberId)
            {
                throw new InvalidOperationException(
                    $"Payload member identity mismatch. HouseholdMemberId {householdMemberId} " +
                    $"does not match HouseholdMemberGuid {householdMemberGuid}.");
            }

            return byGuid ?? byId;
        }

        return byId;
    }
}
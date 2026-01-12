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
    private readonly ILogger<DataListingEventProcessor> _logger;

    public DataCollectionEventProcessor(
        IPackageRepository packageRepository,
        ILogger<DataListingEventProcessor> logger)
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
            var householdMemberGuid = Guid.Parse(memberToRemove.HouseholdMemberGuid);
            var currentHouseholdMember = packageEventHousehold.Members.SingleOrDefault(hm => hm.HouseholdMemberGuid == householdMemberGuid);
            if (currentHouseholdMember == null)
            {
                throw new KeyNotFoundException($"Unable to locate household member {memberToRemove.HouseholdMemberGuid} for marking as not current");
            }
            currentHouseholdMember.SetToRemoved(memberToRemove.StatusDetails.RemovedReason);
        }
    }

    private void ProcessHouseholdMemberChanges(PackageEventHousehold packageEventHousehold, DataCollectionEventDetail deserialisedPayload)
    {
        foreach (var memberToUpdate in deserialisedPayload.Members.Where(hm => hm.HouseholdMemberGuid != "NEWGUID"))
        {
            var householdMemberGuid = Guid.Parse(memberToUpdate.HouseholdMemberGuid);
            var currentHouseholdMember = packageEventHousehold.Members.SingleOrDefault(hm => hm.HouseholdMemberGuid == householdMemberGuid);
            if (currentHouseholdMember == null)
            {
                throw new KeyNotFoundException($"Unable to locate household member {memberToUpdate.HouseholdMemberGuid} for marking as not current");
            }

            var changeAttributeList = memberToUpdate.HouseholdMemberAttributes.Where(ha => ha.ValueState == "Dirty");
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
}
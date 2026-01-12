using Newtonsoft.Json;
using Staging.Domain.AggregatesModel.PackageAggregate;
using Staging.Domain.Models;
using Staging.Domain.ProcessorActivities;
using Staging.Domain.SeedWork;
using Staging.Domain.Services;

namespace Staging.API.Infrastructure.Processors;

public class DataListingEventProcessor
    : PayloadProcessor<DataListingEventPayloadActivity>
{
    private readonly IPackageRepository _packageRepository;
    private readonly ILogger<DataListingEventProcessor> _logger;

    public DataListingEventProcessor(
        IPackageRepository packageRepository,
        ILogger<DataListingEventProcessor> logger)
    {
        _packageRepository = packageRepository ?? throw new ArgumentNullException(nameof(packageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<ExecutionResult> ExecuteCoreAsync(DataListingEventPayloadActivity activity, CancellationToken cancellationToken)
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

        var deserialisedPayload = JsonConvert.DeserializeObject<DataListingEventDetail>(activity.PackageEventHouseholdSynch.Payload);

        try
        {
            var hasNewHouseholdClassification = !String.IsNullOrWhiteSpace(deserialisedPayload.newHouseholdClassification);
            var hasNewCommunityClassification = !String.IsNullOrWhiteSpace(deserialisedPayload.newCommunityClassification);

            if (hasNewHouseholdClassification)
            {
                ProcessHouseholdClassificationChange(activity, deserialisedPayload);
            }
            ProcessOtherChanges(activity, deserialisedPayload, hasNewCommunityClassification);

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

    private void ProcessOtherChanges(DataListingEventPayloadActivity activity, DataListingEventDetail deserialisedPayload, bool hasNewCommunityClassification)
    {
        if (hasNewCommunityClassification)
        {
            activity.PackageEventHousehold.SetListingToListingAndCBCSynched(deserialisedPayload.newCommunityClassification);
        }
        if (!String.IsNullOrWhiteSpace(deserialisedPayload.villageName))
        {
            activity.PackageEventHousehold.SetAttributeValue("Village Name", deserialisedPayload.villageName);
        }
        if (!String.IsNullOrWhiteSpace(deserialisedPayload.cardId))
        {
            activity.PackageEventHousehold.SetAttributeValue("Listing Card ID", deserialisedPayload.cardId);
        }
        if (!String.IsNullOrWhiteSpace(deserialisedPayload.newHouseholdHead))
        {
            activity.PackageEventHousehold.SetAttributeValue("Household Head", deserialisedPayload.newHouseholdHead);
        }
        if (!String.IsNullOrWhiteSpace(deserialisedPayload.newContactNumber))
        {
            activity.PackageEventHousehold.SetAttributeValue("Contact Number", deserialisedPayload.newContactNumber);
        }
        if (!String.IsNullOrWhiteSpace(deserialisedPayload.postalAddress))
        {
            activity.PackageEventHousehold.SetAttributeValue("Listing Physical Address", deserialisedPayload.postalAddress);
        }
        if (!String.IsNullOrWhiteSpace(deserialisedPayload.newhouseholdSize))
        {
            activity.PackageEventHousehold.SetAttributeValue("Listing Household Size", deserialisedPayload.newhouseholdSize);
        }
    }

    private void ProcessHouseholdClassificationChange(DataListingEventPayloadActivity activity, DataListingEventDetail deserialisedPayload)
    {
        if (String.Equals(deserialisedPayload.newHouseholdClassification, "Disolved"))
        {
            activity.PackageEventHousehold.SetToDisolvedHousehold();
        }
        if (String.Equals(deserialisedPayload.newHouseholdClassification, "Duplicate"))
        {
            activity.PackageEventHousehold.SetToDuplicateHousehold();
        }
    }
}
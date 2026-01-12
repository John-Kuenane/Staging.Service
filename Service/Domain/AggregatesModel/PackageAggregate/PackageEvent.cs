using Newtonsoft.Json;
using Staging.Domain.Events;
using Staging.Domain.Models;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEvent
    : Entity, IAggregateRoot
{
    public int PackageStatusId { get; private set; }

    public int? PackageSubStatusId { get; private set; }

    public DateTime? StagePreparationClosed { get; private set; }

    public DateTime? DataManagementStageClosed { get; private set; }

    public DateTime? DataAcceptanceStageClosed { get; private set; }

    public DateTime? GatewayStageClosed { get; private set; }

    public Guid OrgUnitId { get; private set; }
    public string OrgUnitName { get; private set; }

    public int? LastTemporaryHouseholdId { get; private set; }

    private List<PackageEventDevice> _devices;
    public IEnumerable<PackageEventDevice> Devices => _devices.AsReadOnly();

    private List<PackageEventHousehold> _households;
    public IEnumerable<PackageEventHousehold> Households => _households.AsReadOnly();

    private List<PackageEventDataFlag> _flags;
    public IEnumerable<PackageEventDataFlag> Flags => _flags.AsReadOnly();

    protected PackageEvent()
    {
        _devices = new List<PackageEventDevice>();
        _households = new List<PackageEventHousehold>();
        _flags = new List<PackageEventDataFlag>();
    }

    public PackageEvent(Guid orgUnitId, string orgUnitName)
    {
        PackageStatusId = PackageStatus.StagePreparation.Id;

        OrgUnitId = orgUnitId;
        OrgUnitName = orgUnitName;

        LastTemporaryHouseholdId = 0;
    }

    public PackageEventHousehold AddNewListingHousehold(string deviceId, string payload)
    {
        var deserialisedPayload = JsonConvert.DeserializeObject<DataListingEventDetail>(payload);

        if(LastTemporaryHouseholdId.HasValue)
            LastTemporaryHouseholdId += 1;
        else
            LastTemporaryHouseholdId = 800000 + (Id * 100);

        var newHousehold = new PackageEventHousehold(
            householdId: LastTemporaryHouseholdId.Value,
            householdGuid: Guid.NewGuid(),
            communityClassification: deserialisedPayload.newCommunityClassification,
            householdHead: deserialisedPayload.householdHead,
            contactNumber: deserialisedPayload.contactNumber,
            physicalAddress: deserialisedPayload.postalAddress,
            villageName: deserialisedPayload.villageName
        );
        
        newHousehold.SetToNewHousehold();
        _households.Add(newHousehold);

        return newHousehold;
    }

    public PackageEventHousehold AddNewCollectionHousehold(string deviceId, string payload)
    {
        var deserialisedPayload = JsonConvert.DeserializeObject<DataListingEventDetail>(payload);

        if (LastTemporaryHouseholdId.HasValue)
            LastTemporaryHouseholdId += 1;
        else
            LastTemporaryHouseholdId = 800000 + (Id * 100);

        var newHousehold = new PackageEventHousehold(
            householdId: LastTemporaryHouseholdId.Value,
            householdGuid: Guid.NewGuid(),
            communityClassification: "UNIDENTIFIED",
            householdHead: deserialisedPayload.householdHead,
            contactNumber: deserialisedPayload.contactNumber,
            physicalAddress: deserialisedPayload.postalAddress,
            villageName: deserialisedPayload.villageName
        );
        newHousehold.SetToNewHousehold();
        newHousehold.SetCollectionToEnumerated();

        _households.Add(newHousehold);

        return newHousehold;
    }

    public PackageEventDataFlag AddDataFlag(
        int packageEventHouseholdId,
        PersonIdentifier requester,
        DataFlagType dataFlagType,
        DataFlagSubType? dataFlagSubType,
        string subject,
        string description, 
        DateTime? issueDate,
        Priority priority,
        Group? group,
        string vendor,
        bool systemGenerated)
    {
        var newDataFlag = new PackageEventDataFlag(
            packageEventHouseholdId: packageEventHouseholdId,
            requester,
            dataFlagType,
            dataFlagSubType,
            subject, 
            description,
            issueDate,
            priority,
            group,
            systemGenerated
        );
        _flags.Add(newDataFlag);

        AddDataFlagAddedDomainEvent(newDataFlag, vendor);

        return newDataFlag;
    }

    public void ProcessPayloads()
    {
        foreach (var packageEventHousehold in _households)
        {
            foreach (var packageEventHouseholdSynch in packageEventHousehold.Synchs.Where(s => s.PayloadProcessedId == 1 || s.PayloadProcessedId == 3))
            {
                packageEventHouseholdSynch.SetToBeProcessed();
                AddPayloadSynchronisedDomainEvent(packageEventHousehold, packageEventHouseholdSynch);
            }

        }
    }

    public void ChangeSubStatusToDataCollection()
    {
        PackageSubStatusId = PackageSubStatus.DataCollection.Id;
    }

    public void ChangeStatusToAccepted()
    {
        PackageStatusId = PackageStatus.DataAcceptance.Id;
        DataManagementStageClosed = DateTime.UtcNow;
    }

    public void ChangeStatusToGateway()
    {
        PackageStatusId = PackageStatus.Gateway.Id;
        DataAcceptanceStageClosed = DateTime.UtcNow;
    }

    public void ChangeHouseholdStatusToAccepted(int packageEventHouseholdId)
    {
        var packageEventHousehod = _households.FirstOrDefault(h => h.Id == packageEventHouseholdId);
        if (packageEventHousehod == null)
        {
            throw new InvalidOperationException($"Household with ID {packageEventHouseholdId} not found.");
        }
        packageEventHousehod.SetStatusToAccepted();
    }

    public void ChangeHouseholdStatusToRejected(int packageEventHouseholdId)
    {
        var packageEventHousehod = _households.FirstOrDefault(h => h.Id == packageEventHouseholdId);
        if (packageEventHousehod == null)
        {
            throw new InvalidOperationException($"Household with ID {packageEventHouseholdId} not found.");
        }
        packageEventHousehod.SetStatusToAccepted();
    }

    private void AddPayloadSynchronisedDomainEvent(PackageEventHousehold packageEventHousehold, PackageEventHouseholdSynch packageEventHouseholdSynch)
    {
        var payloadSynchronisedDomainEvent = new PayloadSynchronisedDomainEvent(packageEventHousehold, packageEventHouseholdSynch);
        this.AddDomainEvent(payloadSynchronisedDomainEvent);
    }

    private void AddDataFlagAddedDomainEvent(PackageEventDataFlag dataFlag, string vendor)
    {
        var dataFlagAddedDomainEvent = new DataFlagAddedDomainEvent(this, dataFlag, vendor);
        this.AddDomainEvent(dataFlagAddedDomainEvent);
    }
}
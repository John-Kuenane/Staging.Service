using Staging.Domain.Events;
using Staging.Domain.Models;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHousehold
    : Entity, IAggregateRoot
{
    public int HouseholdId { get; private set; }
    public Guid HouseholdGuid { get; private set; }

    public string CommunityClassification { get; private set; }

    public string HouseholdHead { get; private set; }

    public string ContactNumber { get; private set; }

    public string PhysicalAddress { get; private set; }
    public string VillageName { get; private set; }

    public int ListingStatusId { get; private set; }

    public DateTime? ListingStatusDate { get; private set; }

    public int CollectionStatusId { get; private set; }

    public DateTime? CollectionStatusDate { get; private set; }

    public StatusChange Accepted { get; private set; }

    public StatusChange Rejected { get; private set; }

    public string Comments { get; private set; }


    private List<PackageEventHouseholdMember> _members;
    public IEnumerable<PackageEventHouseholdMember> Members => _members.AsReadOnly();

    private List<PackageEventHouseholdAttribute> _attributes;
    public IEnumerable<PackageEventHouseholdAttribute> Attributes => _attributes.AsReadOnly();

    private List<PackageEventHouseholdSynch> _synchs;
    public IEnumerable<PackageEventHouseholdSynch> Synchs => _synchs.AsReadOnly();

    protected PackageEventHousehold()
    {
        _members = new List<PackageEventHouseholdMember>();
        _attributes = new List<PackageEventHouseholdAttribute>();
        _synchs = new List<PackageEventHouseholdSynch>();
    }

    public PackageEventHousehold(int householdId, Guid householdGuid, string communityClassification, string householdHead, string contactNumber, string physicalAddress, string villageName)
    {
        HouseholdId = householdId;
        HouseholdGuid = householdGuid;
        CommunityClassification = communityClassification;
        HouseholdHead = householdHead;
        ContactNumber = contactNumber;
        PhysicalAddress = physicalAddress;
        VillageName = villageName;

        ListingStatusId = ListingStatus.NoStatus.Id;
        CollectionStatusId = CollectionStatus.NoStatus.Id;

        Accepted = new StatusChange(false);
        Rejected = new StatusChange(false);

        _members = new List<PackageEventHouseholdMember>();
        _attributes = new List<PackageEventHouseholdAttribute>();
        _synchs = new List<PackageEventHouseholdSynch>();
    }

    public void SetToNewHousehold()
    {
        ListingStatusId = ListingStatus.NewHouseholdSynched.Id;
        ListingStatusDate = DateTime.UtcNow;

        SetAttributeValue("Household Classification", "New");
    }

    public void SetToDisolvedHousehold()
    {
        ListingStatusId = ListingStatus.DisolvedOrDuplicateHouseholdSynched.Id;
        ListingStatusDate = DateTime.UtcNow;

        SetAttributeValue("Household Classification", "Disolved");
    }

    public void SetToDuplicateHousehold()
    {
        ListingStatusId = ListingStatus.DisolvedOrDuplicateHouseholdSynched.Id;
        ListingStatusDate = DateTime.UtcNow;

        SetAttributeValue("Household Classification", "Duplicate");
    }

    public void SetListingToListingAndCBCSynched(string communityClassification)
    {
        ListingStatusId = ListingStatus.ListingAndCBCSynched.Id;
        ListingStatusDate = DateTime.UtcNow;

        SetAttributeValue("Community Classification", communityClassification);
    }

    public void SetCollectionToEnumerated()
    {
        CollectionStatusId = CollectionStatus.EnumerationSynched.Id;
        CollectionStatusDate = DateTime.UtcNow;
    }

    public void SetStatusToAccepted()
    {
        Accepted = new StatusChange(true);
        Rejected = new StatusChange(false);
    }

    public void SetStatusToRejected()
    {
        Accepted = new StatusChange(false);
        Rejected = new StatusChange(true);
    }

    public PackageEventHouseholdSynch AddSynchronisation(string deviceId, string payload)
    {
        var newSynchronisation = new PackageEventHouseholdSynch(deviceId, payload);
        _synchs.Add(newSynchronisation);

        // 🔑 Domain event
        AddPayloadSynchronisedDomainEvent(newSynchronisation);

        return newSynchronisation;
    }

    public void SetAttributeValue(string attributeKey, string value)
    {
        var attributeValue = _attributes.Where(a => a.AttributeKey == attributeKey)
            .SingleOrDefault();

        if (attributeValue == null)
        {
            attributeValue = new PackageEventHouseholdAttribute(attributeKey, new AttributeValue("", "", ""));
            _attributes.Add(attributeValue);
        }

        attributeValue.UpdateValue(new AttributeValue("", "", value));
    }

    public PackageEventHouseholdMember AddNewMember(DataCollectionEventMember memberPayload)
    {
        var newHouseholdMember = new PackageEventHouseholdMember(
            householdMemberId: 0,
            householdMemberGuid: Guid.NewGuid(),
            firstName: memberPayload.Name,
            surname: memberPayload.Surname,
            idDocumentType: "NOT SET",
            identificationNumber: "NOT SET",
            dateOfBirth: null,
            gender: "NOT SET"
        );

        newHouseholdMember.SetToNew();
        _members.Add(newHouseholdMember);

        return newHouseholdMember;
    }

    private void AddPayloadSynchronisedDomainEvent(PackageEventHouseholdSynch synchRecord)
    {
        var payloadSynchronisedDomainEvent = new PayloadSynchronisedDomainEvent(this, synchRecord);
        this.AddDomainEvent(payloadSynchronisedDomainEvent);
    }
}
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHousehold
    : Entity
{
    public Guid HouseholdId { get; private set; }

    public string Village { get; private set; }

    public string HouseholdHead { get; private set; }

    public string ContactNumber { get; private set; }

    public string Address { get; private set; }

    public StatusChange Enumerated { get; private set; }

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

    public PackageEventHousehold(Guid householdId, string village, string householdHead, string contactNumber, string address)
    {
        HouseholdId = householdId;
        Village = village;
        HouseholdHead = householdHead;
        ContactNumber = contactNumber;
        Address = address;

        Enumerated = new StatusChange(false);
        Accepted = new StatusChange(false);
        Rejected = new StatusChange(false);
    }
}
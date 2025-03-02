using Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventDataFlag
    : Entity
{
    public Guid HouseholdId { get; private set; }

    public Guid? HouseholdMemberId { get; private set; }

    public int DataFlagTypeId { get; private set; }

    public bool SystemGenerated { get; private set; }

    public StatusChange FlagResolved { get; private set; }

    public StatusChange FlagDeferred { get; private set; }

    public string Description { get; private set; }

    private List<PackageEventDataFlagComment> _comments;
    public IEnumerable<PackageEventDataFlagComment> Comments => _comments.AsReadOnly();

    protected PackageEventDataFlag()
    {
        _comments = new List<PackageEventDataFlagComment>();
    }

    public PackageEventDataFlag(Guid householdId, Guid? householdMemberId, DataFlagType dataFlagType, string description, bool systemGenerated)
    {
        HouseholdId = householdId;
        HouseholdMemberId = householdMemberId;
        DataFlagTypeId = dataFlagType.Id;
        Description = description;

        FlagResolved = new StatusChange(false);
        FlagDeferred = new StatusChange(false);
        
        SystemGenerated = systemGenerated;
    }
}
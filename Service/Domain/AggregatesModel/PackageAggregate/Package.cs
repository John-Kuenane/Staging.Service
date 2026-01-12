namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class Package
    : Entity, IAggregateRoot
{
    public int PackageTypeId { get; private set; }

    public StatusChange PackageOpened { get; private set; }

    public StatusChange PackageClosed { get; private set; }

    public Guid OrgUnitId { get; private set; }

    public string ParentOrgunitName { get; private set; }

    public string UniqueCode { get; private set; }

    public string Description { get; private set; }

    public string Vendor { get; private set; }

    public bool System { get; private set; }

    public int? FormId { get; private set; }

    private List<PackageEvent> _events;
    public IEnumerable<PackageEvent> Events => _events.AsReadOnly();

    protected Package()
    {
        _events = new List<PackageEvent>();
    }

    public Package(PackageType packageType, Guid orgUnitId, string uniqueCode, string description)
    {
        PackageTypeId = packageType.Id;

        OrgUnitId = orgUnitId;
        UniqueCode = uniqueCode;
        Description = description;

        PackageOpened = new StatusChange(true);
        PackageClosed = new StatusChange(false);
    }

    public PackageEventDataFlag AddDataFlag(
        int packageEventId,
        int packageEventHouseholdId,
        PersonIdentifier requester,
        DataFlagType dataFlagType,
        DataFlagSubType? dataFlagSubType,
        string subject,
        string description,
        DateTime? issueDate,
        Priority priority,
        Group? group,
        bool systemGenerated)
    {
        var packageEvent = _events.SingleOrDefault(e => e.Id == packageEventId);
        if (packageEvent == null)
        {
            throw new KeyNotFoundException($"Unable to locate package event {packageEventId}");
        }
        else
        {
            var packageEventHousehold = packageEvent.Households.SingleOrDefault(e => e.Id == packageEventHouseholdId);
            if (packageEventHousehold == null)
            {
                throw new KeyNotFoundException($"Unable to locate package event household {packageEventHouseholdId}");
            }

            var dataFlag = packageEvent.AddDataFlag(
                packageEventHouseholdId, 
                requester,
                dataFlagType, 
                dataFlagSubType,
                subject,
                description,
                issueDate,
                priority,
                group,
                Vendor,
                systemGenerated);

            return dataFlag;
        }
    }
}
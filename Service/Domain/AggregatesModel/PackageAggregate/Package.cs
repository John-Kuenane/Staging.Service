using Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class Package
    : Entity, IAggregateRoot
{
    public int PackageTypeId { get; private set; }

    public StatusChange PackageOpened { get; private set; }

    public StatusChange PackageClosed { get; private set; }

    public Guid OrgUnitId { get; private set; }

    public string UniqueCode { get; private set; }

    public string Description { get; private set; }

    public bool System { get; private set; }

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
}

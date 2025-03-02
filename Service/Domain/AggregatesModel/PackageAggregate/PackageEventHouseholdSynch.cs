namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHouseholdSynch
    : Entity
{
    public string DeviceId { get; private set; }

    public string Payload { get; private set; }

    private List<PackageEventHouseholdSynchMetaAttribute> _metaAttributes;
    public IEnumerable<PackageEventHouseholdSynchMetaAttribute> MetaAttributes => _metaAttributes.AsReadOnly();

    protected PackageEventHouseholdSynch()
    {
        _metaAttributes = new List<PackageEventHouseholdSynchMetaAttribute>();
    }

    public PackageEventHouseholdSynch(string payload)
    {
        Payload = payload;
    }
}
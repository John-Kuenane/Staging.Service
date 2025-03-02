namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHouseholdSynchMetaAttribute
    : Entity
{
    public int MetaAttributeTypeId { get; private set; }

    public string MetaAttributeValue { get; private set; }

    protected PackageEventHouseholdSynchMetaAttribute()
    {
    }

    public PackageEventHouseholdSynchMetaAttribute(int metaAttributeTypeId, string metaAttributeValue)
    {
        MetaAttributeTypeId = metaAttributeTypeId;
        MetaAttributeValue = metaAttributeValue;
    }
}
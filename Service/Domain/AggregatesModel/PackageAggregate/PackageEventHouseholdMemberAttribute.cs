using Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHouseholdMemberAttribute
    : Entity
{
    public string AttributeKey { get; private set; }

    public AttributeValue Original { get; private set; }
    public AttributeValue Modified { get; private set; }

    public bool ValueModified { get; private set; }

    protected PackageEventHouseholdMemberAttribute()
    {
    }

    public PackageEventHouseholdMemberAttribute(string attributeKey, AttributeValue original)
    {
        AttributeKey = attributeKey;
        Original = original;
    }
}
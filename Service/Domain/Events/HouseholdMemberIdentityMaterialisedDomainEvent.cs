using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Domain.Events;

public class HouseholdMemberIdentityMaterialisedDomainEvent : INotification
{
    public PackageEventHouseholdMember PackageEventHouseholdMember { get; }

    public string RequestKey { get; }

    public HouseholdMemberIdentityMaterialisedDomainEvent(
        PackageEventHouseholdMember packageEventHouseholdMember, 
        string requestKey)
    {
        PackageEventHouseholdMember = packageEventHouseholdMember;
        RequestKey = requestKey;
    }
}

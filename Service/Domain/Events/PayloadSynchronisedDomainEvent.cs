using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Domain.Events;

public class PayloadSynchronisedDomainEvent : INotification
{
    public PackageEventHousehold PackageEventHousehold { get; }

    public PackageEventHouseholdSynch PackageEventHouseholdSynch { get; }

    public PayloadSynchronisedDomainEvent(
        PackageEventHousehold packageEventHousehold, 
        PackageEventHouseholdSynch packageEventHouseholdSynch)
    {
        PackageEventHousehold = packageEventHousehold;
        PackageEventHouseholdSynch = packageEventHouseholdSynch;
    }
}

using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Domain.Events;

public class DataFlagAddedDomainEvent : INotification
{
    public string Vendor { get; }

    public PackageEvent PackageEvent { get; }

    public PackageEventDataFlag PackageEventDataFlag { get; }

    public DataFlagAddedDomainEvent(
        PackageEvent packageEvent,
        PackageEventDataFlag packageEventDataFlag,
        string vendor)
    {
        PackageEvent = packageEvent;
        PackageEventDataFlag = packageEventDataFlag;
        this.Vendor = vendor;
    }
}

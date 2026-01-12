namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventDevice
    : Entity
{
    public string DeviceId { get; private set; }

    public string EnumeratorName { get; private set; }

    protected PackageEventDevice()
    {
    }

    public PackageEventDevice(string deviceId, string enumeratorName)
    {
        DeviceId = deviceId;
        EnumeratorName = enumeratorName;
    }
}
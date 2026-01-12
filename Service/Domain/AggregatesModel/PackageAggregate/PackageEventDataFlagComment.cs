namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventDataFlagComment
    : Entity
{
    public Guid? OriginatorId { get; private set; }

    public string DeviceId { get; private set; }

    public string Comment { get; private set; }

    protected PackageEventDataFlagComment()
    {
    }

    public PackageEventDataFlagComment(Guid? originatorId, string deviceId, string comment)
    {
        OriginatorId = originatorId;
        DeviceId = deviceId;
        Comment = comment;
    }
}
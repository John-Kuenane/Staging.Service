using System.ComponentModel.DataAnnotations.Schema;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHouseholdSynch
    : Entity
{
    public string DeviceId { get; private set; }

    public string Payload { get; private set; }

    public int PayloadProcessedId { get; private set; }

    public DateTime? PayloadProcessedDate { get; private set; }

    public DateTime? PayloadStatusDate { get; private set; }

    public string FailureMessage { get; private set; }

    private List<PackageEventHouseholdSynchMetaAttribute> _metaAttributes;
    public IEnumerable<PackageEventHouseholdSynchMetaAttribute> MetaAttributes => _metaAttributes.AsReadOnly();

    [NotMapped]
    public bool IsFinalised =>
        PayloadProcessedId == PayloadProcessedStatus.Processed.Id ||
        PayloadProcessedId == PayloadProcessedStatus.Failed.Id;

    [NotMapped] public bool IsPending => PayloadProcessedId == PayloadProcessedStatus.Pending.Id;
    [NotMapped] public bool IsProcessing => PayloadProcessedId == PayloadProcessedStatus.Processing.Id;

    protected PackageEventHouseholdSynch()
    {
        _metaAttributes = new List<PackageEventHouseholdSynchMetaAttribute>();
    }

    public PackageEventHouseholdSynch(string deviceId, string payload)
    {
        DeviceId = deviceId;
        Payload = payload;

        PayloadProcessedId = PayloadProcessedStatus.Pending.Id;
    }

    public bool TrySetToProcessing()
    {
        if (PayloadProcessedId != PayloadProcessedStatus.Pending.Id)
            return false;

        PayloadProcessedId = PayloadProcessedStatus.Processing.Id;
        PayloadProcessedDate = null;
        PayloadStatusDate = DateTime.UtcNow;

        return true;
    }

    public void SetToBeProcessed()
    {
        PayloadProcessedId = PayloadProcessedStatus.NotProcessed.Id;
        PayloadProcessedDate = null;
        PayloadStatusDate = DateTime.UtcNow;
    }

    public void SetToProcessed()
    {
        if (PayloadProcessedId != PayloadProcessedStatus.Processing.Id)
            throw new InvalidOperationException("Invalid state transition");

        PayloadProcessedId = PayloadProcessedStatus.Processed.Id;
        PayloadProcessedDate = DateTime.UtcNow;
    }

    public void SetToPending()
    {
        PayloadProcessedId = PayloadProcessedStatus.Pending.Id;
        PayloadProcessedDate = null;
        PayloadStatusDate = DateTime.UtcNow;
    }

    public void SetToFailed(string message)
    {
        PayloadProcessedId = PayloadProcessedStatus.Failed.Id;
        PayloadProcessedDate = null;
        PayloadStatusDate = DateTime.UtcNow;
        FailureMessage = message;
    }
}
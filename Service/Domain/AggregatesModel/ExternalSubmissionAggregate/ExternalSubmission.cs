namespace Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;

using System;

public class ExternalSubmission
    : Entity
{
    public string Vendor { get; private set; }
    public string Payload { get; private set; }
    public string Status { get; private set; } = "Pending";
    public string? ExternalId { get; private set; }
    public string? Message { get; private set; }
    public int AttemptCount { get; private set; }
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastAttemptAt { get; private set; }

    public ExternalSubmissionType Type { get; private set; }

    public string RequestKey { get; private set; }

    public bool IsFinalised => Status == "Success";

    private ExternalSubmission() { } // EF

    internal ExternalSubmission(ExternalSubmissionType type, string vendor, string payload, string requestKey)
    {
        Type = type;
        Vendor = vendor;
        Payload = payload;
        RequestKey = requestKey;
    }

    public bool CanRetry(int maxAttempts)
        => Status != "Success" && AttemptCount < maxAttempts;

    public bool TrySetToProcessing()
    {
        if (Status != "Pending" && Status != "Failed")
            return false;

        Status = "Processing";
        LastAttemptAt = DateTime.UtcNow;
        return true;
    }

    internal void MarkSuccess(string? externalId, string? response)
    {
        Status = "Success";
        ExternalId = externalId;
        Message = response;
        AttemptCount++;
        LastAttemptAt = DateTime.UtcNow;
    }

    internal void MarkFailure(string reason, string? response)
    {
        Status = "Failed";
        Message = $"{reason}: {response}";
        AttemptCount++;
        LastAttemptAt = DateTime.UtcNow;
    }
}

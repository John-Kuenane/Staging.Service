using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public sealed class IdentityVerification : ValueObject
{
    public string Status { get; private set; } // Pending|Verified|Failed
    public DateTime? RecordedAt { get; private set; }
    public string? Message { get; private set; }
    public string? Reference { get; private set; }

    public MemberIdentifier? VerifiedIdentity { get; private set; }

    private IdentityVerification() { }

    private IdentityVerification(string status, DateTime? recordedAt, string? message, string? reference, MemberIdentifier? verifiedIdentity)
    {
        Status = status;
        RecordedAt = DateTime.UtcNow;
        Message = message;
        Reference = reference;
        VerifiedIdentity = verifiedIdentity;
    }

    public static IdentityVerification Pending() => new(IdentityVerificationStatus.Pending, null, null, null, null);
    public static IdentityVerification Submitted()
        => new(IdentityVerificationStatus.Submitted, DateTime.UtcNow, null, null, null);
    public static IdentityVerification Verified(MemberIdentifier verifiedIdentity, string? message = null, string? reference = null)
        => new(IdentityVerificationStatus.Verified, DateTime.UtcNow, message, reference, verifiedIdentity);
    public static IdentityVerification Failed(MemberIdentifier attemptedIdentity, string message, string? reference = null)
        => new(IdentityVerificationStatus.Failed, DateTime.UtcNow, message, reference, attemptedIdentity);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Status;
        yield return Message;
        yield return Reference;
        yield return VerifiedIdentity;
    }
}


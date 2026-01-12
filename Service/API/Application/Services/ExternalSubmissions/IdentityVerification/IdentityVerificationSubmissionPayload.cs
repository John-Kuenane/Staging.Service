using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Application.Services.ExternalSubmissions.IdentityVerification;

public sealed record IdentityVerificationSubmissionPayload(
    int MemberId,
    string Provider,
    MemberIdentifier Identity
);

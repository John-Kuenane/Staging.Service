using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

public interface IIdentityVerificationProvider
{
    string Name { get; } // "NICR" or "Golsabs"

    Task<IdentityVerificationProviderResult> VerifyAsync(
        MemberIdentifier identity,
        CancellationToken ct);
}

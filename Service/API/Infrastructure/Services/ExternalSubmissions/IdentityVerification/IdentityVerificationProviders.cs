namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

public sealed record IdentityVerificationProviders
{
    public ProviderSettings NICR { get; init; } = new();
    public ProviderSettings Golsabs { get; init; } = new();
}

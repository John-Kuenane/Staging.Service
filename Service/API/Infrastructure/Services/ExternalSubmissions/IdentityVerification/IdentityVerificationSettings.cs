namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

public sealed record IdentityVerificationSettings
{
    public string Provider { get; init; } = "NICR";
    public IdentityVerificationProviders Providers { get; init; } = new();
}

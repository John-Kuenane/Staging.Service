namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

public sealed record IdentityVerificationProviderResult(
    bool Success,
    string? Reference,
    string? Message,
    string? RawResponse = null);

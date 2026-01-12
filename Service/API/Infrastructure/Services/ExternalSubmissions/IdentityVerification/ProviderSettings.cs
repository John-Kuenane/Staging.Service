namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

public sealed record ProviderSettings
{
    public string EndPoint { get; init; } = default!;
    public string? ApiKey { get; init; }
}

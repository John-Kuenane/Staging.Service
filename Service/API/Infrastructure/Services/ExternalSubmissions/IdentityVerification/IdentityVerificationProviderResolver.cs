using Microsoft.Extensions.Options;

namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

public sealed class IdentityVerificationProviderResolver : IIdentityVerificationProviderResolver
{
    private readonly IdentityVerificationSettings _settings;
    private readonly IEnumerable<IIdentityVerificationProvider> _providers;

    public IdentityVerificationProviderResolver(
        IOptions<IdentityVerificationSettings> options,
        IEnumerable<IIdentityVerificationProvider> providers)
    {
        _settings = options.Value;
        _providers = providers;
    }

    public IIdentityVerificationProvider Resolve()
        => Resolve(_settings.Provider);

    public IIdentityVerificationProvider Resolve(string providerName)
    {
        var provider = _providers.SingleOrDefault(p =>
            string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));

        return provider ?? throw new InvalidOperationException(
            $"No identity verification provider registered for '{providerName}'.");
    }
}

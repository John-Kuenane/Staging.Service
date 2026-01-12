namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

public interface IIdentityVerificationProviderResolver
{
    IIdentityVerificationProvider Resolve();
    IIdentityVerificationProvider Resolve(string providerName);
}

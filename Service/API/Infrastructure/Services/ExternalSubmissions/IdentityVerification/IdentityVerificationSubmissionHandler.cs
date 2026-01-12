namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification;

using Azure;
using Microsoft.EntityFrameworkCore;
using Staging.API.Application.Services.ExternalSubmissions;
using Staging.API.Application.Services.ExternalSubmissions.IdentityVerification;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;
using Staging.Domain.AggregatesModel.PackageAggregate;
using System.Text.Json;

public sealed class IdentityVerificationSubmissionHandler : IExternalSubmissionHandler
{
    public ExternalSubmissionType SubmissionType
        => ExternalSubmissionType.IdentityVerification;

    private readonly DatabaseContext _db;
    private readonly IIdentityVerificationProviderResolver _resolver;
    private readonly ILogger<IdentityVerificationSubmissionHandler> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IdentityVerificationSubmissionHandler(
        DatabaseContext db,
        IIdentityVerificationProviderResolver resolver,
        ILogger<IdentityVerificationSubmissionHandler> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExternalSubmissionResult> HandleAsync(
        ExternalSubmission submission,
        CancellationToken cancellationToken)
    {
        IdentityVerificationSubmissionPayload payload;

        try
        {
            payload = JsonSerializer.Deserialize<IdentityVerificationSubmissionPayload>(
                submission.Payload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? throw new InvalidOperationException("Payload deserialized to null.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid identity verification payload for SubmissionId={SubmissionId}", submission.Id);

            return new ExternalSubmissionResult
            {
                Success = false,
                ErrorMessage = $"Invalid payload: {ex.Message}"
            };
        }

        var member = await _db.Set<PackageEventHouseholdMember>()
            .SingleOrDefaultAsync(m => m.Id == payload.MemberId, cancellationToken);

        if (member == null)
        {
            _logger.LogError(
                "Member not found for identity verification. MemberId={MemberId}, SubmissionId={SubmissionId}",
                payload.MemberId,
                submission.Id);

            return new ExternalSubmissionResult
            {
                Success = false,
                ErrorMessage = $"Member not found: MemberId={payload.MemberId}"
            };
        }

        if (!member.IdentityForVerification.IsCompleteForVerification())
        {
            _logger.LogInformation(
                "Member identity not complete at processing time. MemberId={MemberId}, SubmissionId={SubmissionId}",
                payload.MemberId,
                submission.Id);

            return new ExternalSubmissionResult
            {
                Success = false,
                ErrorMessage = $"Member identity became incomplete"
            };
        }

        try
        {
            var provider = _resolver.Resolve(payload.Provider); // NICR/Golsabs
            var verifyResult = await provider.VerifyAsync(payload.Identity, cancellationToken);

            if (verifyResult.Success)
            {
                // Update domain state
                member.MarkIdentityVerified(
                    message: verifyResult.Message,
                    reference: verifyResult.Reference);

                await _db.SaveChangesAsync(cancellationToken);

                return new ExternalSubmissionResult
                {
                    Success = true,
                    ExternalId = verifyResult.Reference,
                    ResponsePayload = verifyResult.Message
                };

            }            
            else
            {
                member.MarkIdentityVerificationFailed(
                    message: verifyResult.Message ?? "Verification failed",
                    reference: verifyResult.Reference);

                await _db.SaveChangesAsync(cancellationToken);

                return new ExternalSubmissionResult
                {
                    Success = false,
                    ErrorMessage = verifyResult.Message ?? "Verification failed",
                    ResponsePayload = verifyResult.Reference
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error verifying identity. MemberId={MemberId}, SubmissionId={SubmissionId}, Provider={Provider}",
                payload.MemberId,
                submission.Id,
                payload.Provider);

            return new ExternalSubmissionResult
            {
                Success = false,
                ErrorMessage = $"Unhandled error: {ex.ToString()}"
            };
        }
    }
}


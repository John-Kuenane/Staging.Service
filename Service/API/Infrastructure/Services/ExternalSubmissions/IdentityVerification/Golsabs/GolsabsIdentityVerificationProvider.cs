namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification.Golsabs;

using Microsoft.Extensions.Options;
using RestSharp;
using Staging.Domain.AggregatesModel.PackageAggregate;
using System.Globalization;
using System.Net;
using System.Text.Json;

public sealed class GolsabsIdentityVerificationProvider : IIdentityVerificationProvider
{
    public string Name => "Golsabs";

    private readonly RestClient _client;
    private readonly ProviderSettings _settings;
    private readonly ILogger<GolsabsIdentityVerificationProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GolsabsIdentityVerificationProvider(
        IOptions<IdentityVerificationSettings> options,
        ILogger<GolsabsIdentityVerificationProvider> logger)
    {
        _logger = logger;
        _settings = options.Value.Providers.Golsabs;

        _client = new RestClient(new RestClientOptions(_settings.EndPoint)
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(15)
        });
    }

    public async Task<IdentityVerificationProviderResult> VerifyAsync(
        MemberIdentifier identity,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identity.IdentificationNumber))
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "IdentificationNumber is required.",
                RawResponse: null);
        }

        var request = new RestRequest("citizen", Method.Get)
            .AddQueryParameter("idNumber", identity.IdentificationNumber);

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            request.AddHeader("X-Api-Key", _settings.ApiKey);

        RestResponse response;
        try
        {
            response = await _client.ExecuteAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Caller cancelled (service shutting down etc.)
            throw;
        }
        catch (Exception ex)
        {
            // Unexpected RestSharp/serialization-level exception
            _logger.LogError(ex,
                "Golsabs lookup threw an exception before receiving a response. IdNumberEnding={IdSuffix}",
                SafeSuffix(identity.IdentificationNumber));

            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "Identity verification service is currently unavailable (unexpected error). Please try again later.",
                RawResponse: null);
        }

        // ---- Transport / timeout handling (prevents "0 0") ----
        if (response.ResponseStatus is ResponseStatus.TimedOut)
        {
            _logger.LogWarning(response.ErrorException,
                "Golsabs lookup timed out. TimeoutSeconds={TimeoutSeconds} IdNumberEnding={IdSuffix}",
                15, SafeSuffix(identity.IdentificationNumber));

            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "Identity verification service is temporarily unavailable (timeout). Please try again later.",
                RawResponse: response.Content);
        }

        if (response.ResponseStatus is ResponseStatus.Error or ResponseStatus.Aborted)
        {
            _logger.LogWarning(response.ErrorException,
                "Golsabs lookup failed at transport level. ResponseStatus={ResponseStatus} HttpStatus={HttpStatus} Error={Error} IdNumberEnding={IdSuffix}",
                response.ResponseStatus,
                (int)response.StatusCode,
                response.ErrorMessage,
                SafeSuffix(identity.IdentificationNumber));

            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "Identity verification service is temporarily unavailable. Please try again later.",
                RawResponse: response.Content);
        }
        // ------------------------------------------------------

        if (response.StatusCode != HttpStatusCode.OK)
        {
            // HTTP-level failure (we got a response from server)
            _logger.LogWarning(
                "Golsabs lookup returned non-OK. StatusCode={StatusCode} Body={Body} IdNumberEnding={IdSuffix}",
                (int)response.StatusCode,
                Truncate(response.Content, 2000),
                SafeSuffix(identity.IdentificationNumber));

            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: $"Identity verification service returned an error ({(int)response.StatusCode}). Please try again later.",
                RawResponse: response.Content);
        }

        var raw = response.Content ?? "[]";

        List<GolsabsCitizenDto>? citizens;
        try
        {
            citizens = JsonSerializer.Deserialize<List<GolsabsCitizenDto>>(raw, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Golsabs response JSON. Body={Body}", Truncate(raw, 2000));
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "Failed to parse identity verification response.",
                RawResponse: raw);
        }

        if (citizens == null || citizens.Count == 0)
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "Citizen not found.",
                RawResponse: raw);
        }

        var citizen = citizens[0];

        // Enforce ACTIVE status
        if (!string.Equals(citizen.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: citizen.IDNumber,
                Message: "Citizen is not Active.",
                RawResponse: raw);
        }

        var mismatches = ValidateAgainstInput(identity, citizen);
        if (mismatches.Count > 0)
        {
            // NOTE: This message currently includes PII (names/dob). Consider removing details if stored on the member.
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: citizen.IDNumber,
                Message: "Identity details did not match.",
                RawResponse: raw);
        }

        return new IdentityVerificationProviderResult(
            Success: true,
            Reference: citizen.IDNumber,
            Message: "Verified",
            RawResponse: raw);

        static string SafeSuffix(string idNumber)
            => idNumber.Length <= 4 ? idNumber : idNumber[^4..];

        static string? Truncate(string? s, int max)
            => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s.Substring(0, max));
    }

    private static List<string> ValidateAgainstInput(MemberIdentifier input, GolsabsCitizenDto citizen)
    {
        var issues = new List<string>();

        // Name checks (case-insensitive, trimmed)
        if (!IsSameText(input.FirstName, citizen.FirstName))
            issues.Add($"FirstName differs (input='{input.FirstName}', api='{citizen.FirstName}')");

        if (!IsSameText(input.Surname, citizen.Surname))
            issues.Add($"Surname differs (input='{input.Surname}', api='{citizen.Surname}')");

        // DOB check - Golsabs gives "dd/MM/yyyy"
        if (input.DateOfBirth.HasValue)
        {
            var apiDob = ParseGolsabsBirthDate(citizen.BirthDate);
            if (apiDob == null)
                issues.Add($"BirthDate missing/invalid (api='{citizen.BirthDate}')");
            else if (apiDob.Value.Date != input.DateOfBirth.Value.Date)
                issues.Add($"DateOfBirth differs (input='{input.DateOfBirth:yyyy-MM-dd}', api='{apiDob:yyyy-MM-dd}')");
        }

        // Optional gender check (if we store gender on MemberIdentifier later)
        // if (!IsSameText(input.Gender, citizen.Gender)) ...

        return issues;
    }

    private static bool IsSameText(string? a, string? b)
        => string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

    private static DateTime? ParseGolsabsBirthDate(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return null;

        // example: "10/03/2022"
        if (DateTime.TryParseExact(
                birthDate.Trim(),
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
            return dt;

        // Fallback to general parse if format ever changes
        if (DateTime.TryParse(birthDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return dt;

        return null;
    }
}


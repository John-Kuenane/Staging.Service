namespace Staging.API.Infrastructure.Services.ExternalSubmissions.IdentityVerification.NICR;

using Microsoft.Extensions.Options;
using RestSharp;
using Staging.Domain.AggregatesModel.PackageAggregate;
using System.Globalization;
using System.Net;
using System.Text.Json;

public sealed class NicrIdentityVerificationProvider : IIdentityVerificationProvider
{
    public string Name => "NICR";

    private readonly IdentityVerificationSettings _settingsRoot;
    private readonly ProviderSettings _settings;
    private readonly RestClient _client;
    private readonly ILogger<NicrIdentityVerificationProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NicrIdentityVerificationProvider(
        IOptions<IdentityVerificationSettings> options,
        ILogger<NicrIdentityVerificationProvider> logger)
    {
        _logger = logger;
        _settingsRoot = options.Value;
        _settings = _settingsRoot.Providers.NICR;

        _client = new RestClient(new RestClientOptions(_settings.EndPoint)
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(15)
        });
    }

    public async Task<IdentityVerificationProviderResult> VerifyAsync(
        MemberIdentifier identity,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(identity.IdentificationNumber))
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "IdentificationNumber is required.",
                RawResponse: null);
        }

        // NICR: /apiService.svc/GetPersonByID?key=<SecurityKey>&idNumber=<PersonId>
        // Your config currently has ApiKey in Providers:NICR:ApiKey
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "NICR ApiKey is not configured.",
                RawResponse: null);
        }

        var request = new RestRequest("apiService.svc/GetPersonByID", Method.Get)
            .AddQueryParameter("key", _settings.ApiKey)
            .AddQueryParameter("idNumber", identity.IdentificationNumber);

        var response = await _client.ExecuteAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            _logger.LogWarning("NICR lookup failed: {StatusCode} {Body}",
                response.StatusCode, response.Content);

            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: $"NICR returned {(int)response.StatusCode} {response.StatusCode}",
                RawResponse: response.Content);
        }

        var raw = response.Content ?? "{}";

        NicrPersonDto? person;
        try
        {
            person = JsonSerializer.Deserialize<NicrPersonDto>(raw, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse NICR response: {Body}", raw);
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "Failed to parse NICR response JSON.",
                RawResponse: raw);
        }

        if (person == null)
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: null,
                Message: "NICR returned an empty response.",
                RawResponse: raw);
        }

        // Not found cases
        if (string.Equals(person.Status, "Person Not Found", StringComparison.OrdinalIgnoreCase))
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: person.IDNumber ?? identity.IdentificationNumber,
                Message: "Person Not Found.",
                RawResponse: raw);
        }

        // Enforce ACTIVE only
        if (!string.Equals(person.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: person.IDNumber ?? identity.IdentificationNumber,
                Message: $"Person status is '{person.Status}', expected 'Active'.",
                RawResponse: raw);
        }

        // Strong verification: ensure biographic match (same approach as Golsabs)
        var mismatches = ValidateAgainstInput(identity, person);
        if (mismatches.Count > 0)
        {
            return new IdentityVerificationProviderResult(
                Success: false,
                Reference: person.IDNumber ?? identity.IdentificationNumber,
                Message: "Mismatch: " + string.Join("; ", mismatches),
                RawResponse: raw);
        }

        return new IdentityVerificationProviderResult(
            Success: true,
            Reference: person.IDNumber ?? identity.IdentificationNumber,
            Message: "Active citizen verified",
            RawResponse: raw);
    }

    private static List<string> ValidateAgainstInput(MemberIdentifier input, NicrPersonDto person)
    {
        var issues = new List<string>();

        if (!IsSameText(input.FirstName, person.FirstName))
            issues.Add($"FirstName differs (input='{input.FirstName}', api='{person.FirstName}')");

        if (!IsSameText(input.Surname, person.Surname))
            issues.Add($"Surname differs (input='{input.Surname}', api='{person.Surname}')");

        if (input.DateOfBirth.HasValue)
        {
            var apiDob = ParseDob(person.BirthDate);
            if (apiDob == null)
                issues.Add($"BirthDate missing/invalid (api='{person.BirthDate}')");
            else if (apiDob.Value.Date != input.DateOfBirth.Value.Date)
                issues.Add($"DateOfBirth differs (input='{input.DateOfBirth:yyyy-MM-dd}', api='{apiDob:yyyy-MM-dd}')");
        }

        return issues;
    }

    private static bool IsSameText(string? a, string? b)
        => string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);

    private static DateTime? ParseDob(string? birthDate)
    {
        if (string.IsNullOrWhiteSpace(birthDate))
            return null;

        // NICR says DD/MM/YYYY; examples show "09\/09\/1999" which becomes "09/09/1999" when JSON is parsed
        if (DateTime.TryParseExact(
                birthDate.Trim(),
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
            return dt;

        // fallback
        if (DateTime.TryParse(birthDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            return dt;

        return null;
    }
}

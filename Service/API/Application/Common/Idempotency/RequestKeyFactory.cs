using global::Staging.Domain.Events;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Staging.API.Application.Common.Idempotency;

/// <summary>
/// Deterministic idempotency keys for external submissions.
/// Keep algorithms stable and versioned. Any change must bump the version.
/// </summary>
public static class RequestKeyFactory
{
    private const string Version = "v1";

    /// <summary>
    /// Identity verification: always use the domain event RequestKey.
    /// Throws if missing because "always set RequestKey" is a correctness contract.
    /// </summary>
    public static string ForIdentityVerification(HouseholdMemberIdentityMaterialisedDomainEvent domainEvent)
    {
        if (domainEvent is null) throw new ArgumentNullException(nameof(domainEvent));

        var rk = domainEvent.RequestKey?.Trim();
        if (string.IsNullOrWhiteSpace(rk))
        {
            throw new InvalidOperationException(
                $"Identity materialised event missing RequestKey for member {domainEvent.PackageEventHouseholdMember?.Id}.");
        }

        // Prefix gives you a human-readable hint in logs/DB.
        return $"IV:{Version}:{rk}";
    }

    /// <summary>
    /// DataFlag/Freshdesk: deterministic from stable domain event + data flag fields.
    /// IMPORTANT: Do not include database-generated IDs or "now" timestamps.
    /// </summary>
    public static string ForDataFlagFreshdesk(DataFlagAddedDomainEvent domainEvent)
    {
        if (domainEvent is null) throw new ArgumentNullException(nameof(domainEvent));
        if (domainEvent.PackageEvent is null) throw new InvalidOperationException("DataFlagAddedDomainEvent missing PackageEvent.");
        if (domainEvent.PackageEventDataFlag is null) throw new InvalidOperationException("DataFlagAddedDomainEvent missing PackageEventDataFlag.");

        var df = domainEvent.PackageEventDataFlag;

        // Canonical payload. Keep this stable. If you add/remove fields, bump Version.
        var parts = new Dictionary<string, string?>
        {
            ["pe"] = domainEvent.PackageEvent.Id.ToString(CultureInfo.InvariantCulture),
            ["hh"] = df.PackageEventHouseholdId.ToString(CultureInfo.InvariantCulture),
            ["type"] = df.DataFlagTypeId.ToString(CultureInfo.InvariantCulture),
            ["subtype"] = df.DataFlagSubTypeId?.ToString(CultureInfo.InvariantCulture),
            ["prio"] = df.PriorityId.ToString(CultureInfo.InvariantCulture),
            ["grp"] = df.GroupId?.ToString(CultureInfo.InvariantCulture),
            ["sys"] = df.SystemGenerated ? "1" : "0",
            ["subject"] = df.Subject,
            ["desc"] = df.Description,
            ["issued"] = df.IssueDate.HasValue ? ToUtcIso(df.IssueDate.Value) : null,
            ["vendor"] = domainEvent.Vendor
        };

        var canonical = Canonicalize(parts);
        var hash = Sha256Hex(canonical);

        // Keep it short, consistent, and searchable in DB logs
        return $"DF:{Version}:{domainEvent.PackageEvent.Id}:{hash}";
    }

    /// <summary>
    /// General-purpose deterministic key builder if you want to expand to other submission types later.
    /// </summary>
    public static string Build(string prefix, int packageEventId, IReadOnlyDictionary<string, string?> parts)
    {
        if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("Prefix is required.", nameof(prefix));
        if (parts is null) throw new ArgumentNullException(nameof(parts));

        var canonical = Canonicalize(parts);
        var hash = Sha256Hex(canonical);

        return $"{prefix}:{Version}:{packageEventId}:{hash}";
    }

    private static string Canonicalize(IReadOnlyDictionary<string, string?> parts)
    {
        // Stable ordering + normalized values
        // Format: key=value|key=value|...
        var ordered = parts
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => $"{kv.Key}={Normalize(kv.Value)}");

        return string.Join("|", ordered);
    }

    private static string Normalize(string? value)
    {
        if (value is null) return "null";

        // Trim, collapse whitespace, lowercase for stability
        var trimmed = value.Trim();
        if (trimmed.Length == 0) return "empty";

        var collapsed = CollapseWhitespace(trimmed);
        return collapsed.ToLowerInvariant();
    }

    private static string CollapseWhitespace(string s)
    {
        var sb = new StringBuilder(s.Length);
        var inWs = false;

        foreach (var ch in s)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!inWs)
                {
                    sb.Append(' ');
                    inWs = true;
                }
                continue;
            }

            sb.Append(ch);
            inWs = false;
        }

        return sb.ToString().Trim();
    }

    private static string Sha256Hex(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes); // 64 chars
    }

    private static string ToUtcIso(DateTime dt)
    {
        // Make sure the same value serializes consistently across timezones/kinds
        var utc = dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc) // assume already UTC-like if unspecified
        };
        return utc.ToString("O", CultureInfo.InvariantCulture);
    }
}
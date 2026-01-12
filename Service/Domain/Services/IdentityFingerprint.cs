using Staging.Domain.AggregatesModel.PackageAggregate;
using System.Security.Cryptography;
using System.Text;

namespace Staging.Domain.Services;

public static class IdentityFingerprint
{
    public static string Create(Guid householdMemberGuid, MemberIdentifier identity)
    {
        // Stable canonical string
        var raw =
            $"{householdMemberGuid:D}|{identity.IdentificationNumber?.Trim()}|{identity.FirstName?.Trim()}|{identity.Surname?.Trim()}|{identity.DateOfBirth:yyyy-MM-dd}";

        // Hash it to keep it short
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes); // e.g. "A1B2..."
    }
}

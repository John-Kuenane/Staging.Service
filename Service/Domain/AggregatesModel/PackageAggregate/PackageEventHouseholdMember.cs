using Staging.Domain.Events;
using Staging.Domain.Services;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Numerics;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHouseholdMember
    : Entity
{
    public int HouseholdMemberId { get; private set; }
    public Guid HouseholdMemberGuid { get; private set; }

    public string FirstName { get; private set; }
    public string Surname { get; private set; }
    public string IDDocumentType { get; private set; }
    public string IdentificationNumber { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string Gender { get; private set; }

    [NotMapped]
    public MemberIdentifier RegisteredIdentity =>
        new MemberIdentifier(IdentificationNumber, FirstName, Surname, DateOfBirth);

    public MemberIdentifier? EnumeratedIdentity { get; private set; }

    public IdentityVerification IdentityVerification { get; private set; } = IdentityVerification.Pending();

    [NotMapped]
    public MemberIdentifier IdentityForVerification =>
        EnumeratedIdentity ?? RegisteredIdentity;

    private List<PackageEventHouseholdMemberAttribute> _attributes;
    public IEnumerable<PackageEventHouseholdMemberAttribute> Attributes => _attributes.AsReadOnly();

    protected PackageEventHouseholdMember()
    {
        _attributes = new List<PackageEventHouseholdMemberAttribute>();
    }

    public PackageEventHouseholdMember(int householdMemberId, Guid householdMemberGuid, string firstName, string surname, string idDocumentType, string identificationNumber, DateTime? dateOfBirth, string gender)
    {
        HouseholdMemberId = householdMemberId;
        HouseholdMemberGuid = householdMemberGuid;
        FirstName = firstName;
        Surname = surname;
        IDDocumentType = idDocumentType;
        IdentificationNumber = identificationNumber;
        DateOfBirth = dateOfBirth;
        Gender = gender;

        _attributes = new List<PackageEventHouseholdMemberAttribute>();
    }

    public void MaterialiseEnumeratedIdentity()
    {
        var calculated = CalculateEnumeratedIdentity();

        if (calculated == null)
            return;

        if (EnumeratedIdentity != null && EnumeratedIdentity.Equals(calculated))
            return;

        EnumeratedIdentity = calculated;

        ResetIdentityVerification();

        QueueIdentityVerificationIfReady();
    }

    public void MarkIdentityVerificationPending()
    {
        if (!IdentityForVerification.IsCompleteForVerification())
            throw new InvalidOperationException("Identity is incomplete for verification.");

        IdentityVerification = IdentityVerification.Pending();
    }

    public void MarkIdentityVerificationSubmitted()
    {
        if (!IdentityForVerification.IsCompleteForVerification())
            throw new InvalidOperationException("Identity is incomplete for verification.");

        IdentityVerification = IdentityVerification.Submitted();
    }

    public void MarkIdentityVerified(string? message = null, string? reference = null)
    {
        if (!IdentityForVerification.IsCompleteForVerification())
            throw new InvalidOperationException("Identity is incomplete for verification.");

        IdentityVerification = IdentityVerification.Verified(IdentityForVerification, message, reference);
    }

    public void MarkIdentityVerificationFailed(string message, string? reference = null)
    {
        if (!IdentityForVerification.IsCompleteForVerification())
            throw new InvalidOperationException("Identity is incomplete for verification.");

        IdentityVerification = IdentityVerification.Failed(IdentityForVerification, message, reference);
    }

    public void SetToNew() => SetAttributeValue("Household Member Classification", "New");
    public void SetToRemoved(string reason) => SetAttributeValue("Household Member Classification", reason);

    public void SetAttributeValue(string attributeKey, string value)
    {
        var attributeValue = _attributes.Where(a => a.AttributeKey == attributeKey)
            .FirstOrDefault();

        if (attributeValue == null)
        {
            attributeValue = new PackageEventHouseholdMemberAttribute(attributeKey, new AttributeValue("", "", ""));
            _attributes.Add(attributeValue);
        }

        attributeValue.UpdateValue(new AttributeValue("", "", value));
    }

    private MemberIdentifier? CalculateEnumeratedIdentity()
            {
        var idNumber = GetAttributeValue(IdentityAttributeKeys.IdNumber);
        var firstName = GetAttributeValue(IdentityAttributeKeys.FirstName);
        var surname = GetAttributeValue(IdentityAttributeKeys.Surname);
        var dob = GetAttributeDate(IdentityAttributeKeys.Dob);

        if (string.IsNullOrWhiteSpace(idNumber))
            return null;

        return new MemberIdentifier(
            identificationNumber: idNumber,
            firstName: firstName,
            surname: surname,
            dateOfBirth: dob
        );
    }

    private void ResetIdentityVerification()
    {
        IdentityVerification = IdentityVerification.Pending();
    }

    private string GetAttributeValue(string attributeKey)
    {
        var attributeValue = _attributes.Where(a => a.AttributeKey == attributeKey)
            .FirstOrDefault();

        if (attributeValue == null)
        {
            return string.Empty;
        }

        return attributeValue.Modified.Value;
    }

    private DateTime? GetAttributeDate(string attributeKey)
    {
        var raw = GetAttributeValue(attributeKey);
        if (string.IsNullOrWhiteSpace(raw)) return null;

        if (DateTimeOffset.TryParse(
                raw,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dto))
        {
            var local = TimeZoneInfo.ConvertTime(dto, ZaTz);
            return local.Date; // date-only semantics in ZA time
        }

        return null;
    }

    private void QueueIdentityVerificationIfReady()
    {
        if (!IdentityForVerification.IsCompleteForVerification())
            return;

        var requestKey = IdentityFingerprint.Create(HouseholdMemberGuid, IdentityForVerification);

        this.AddDomainEvent(new HouseholdMemberIdentityMaterialisedDomainEvent(
            packageEventHouseholdMember: this,
            requestKey: requestKey));
    }

    private static class IdentityAttributeKeys
    {
        public const string IdNumber = "Identification Number";
        public const string FirstName = "Member First name";
        public const string Surname = "Member Surname";
        public const string Dob = "Date of Birth";
    }

    private static readonly TimeZoneInfo ZaTz =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "South Africa Standard Time" : "Africa/Johannesburg");
}
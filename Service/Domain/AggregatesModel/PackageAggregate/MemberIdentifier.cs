using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class MemberIdentifier : ValueObject
{
    public string? IdentificationNumber { get; private set; }
    public string? FirstName { get; private set; }
    public string? Surname { get; private set; }
    public DateTime? DateOfBirth { get; private set; }

    private MemberIdentifier() { } // EF

    public MemberIdentifier(
        string? identificationNumber,
        string? firstName,
        string? surname,
        DateTime? dateOfBirth)
    {
        IdentificationNumber = identificationNumber;
        FirstName = firstName;
        Surname = surname;
        DateOfBirth = dateOfBirth;
    }

    public bool IsCompleteForVerification()
    {
        return !string.IsNullOrWhiteSpace(IdentificationNumber)
            && !string.IsNullOrWhiteSpace(FirstName)
            && !string.IsNullOrWhiteSpace(Surname)
            && DateOfBirth.HasValue;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return IdentificationNumber;
        yield return FirstName;
        yield return Surname;
        yield return DateOfBirth;
    }
}

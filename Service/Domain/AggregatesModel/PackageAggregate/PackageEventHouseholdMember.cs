namespace MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageEventHouseholdMember
    : Entity
{
    public string FirstName { get; private set; }

    public string Surname { get; private set; }

    public string IDDocumentType { get; private set; }

    public string IdentificationNumber { get; private set; }

    private List<PackageEventHouseholdMemberAttribute> _attributes;
    public IEnumerable<PackageEventHouseholdMemberAttribute> Attributes => _attributes.AsReadOnly();

    protected PackageEventHouseholdMember()
    {
        _attributes = new List<PackageEventHouseholdMemberAttribute>();
    }

    public PackageEventHouseholdMember(string firstName, string surname, string iDDocumentType, string identificationNumber)
    {
        FirstName = firstName;
        Surname = surname;
        IDDocumentType = iDDocumentType;
        IdentificationNumber = identificationNumber;
    }
}
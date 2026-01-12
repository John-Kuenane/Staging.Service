using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PersonIdentifier : ValueObject
{
    public string FullName { get; private set; }
    public string Email { get; private set; }

    public PersonIdentifier() { }

    public PersonIdentifier(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        // Using a yield return statement to return each element one at a time
        yield return FullName;
        yield return Email;
    }
}

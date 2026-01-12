using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.FormAggregate;

public class Version : ValueObject
{
    public int Major { get; private set; }
    public int Minor { get; private set; }

    public Version() { }

    public Version(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        // Using a yield return statement to return each element one at a time
        yield return Major;
        yield return Minor;
    }
}
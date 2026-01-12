using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class PackageSubStatus
        : Enumeration
{
    public static PackageSubStatus DataListing = new PackageSubStatus(1, "Data Listing");
    public static PackageSubStatus DataCollection = new PackageSubStatus(2, "Data Collection");
    
public PackageSubStatus(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<PackageSubStatus> List() =>
        new[] { DataListing, DataCollection };

    public static PackageSubStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Package Sub Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static PackageSubStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Package Sub Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}
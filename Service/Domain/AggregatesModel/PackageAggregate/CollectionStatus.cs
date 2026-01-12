using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.PackageAggregate;

public class CollectionStatus
        : Enumeration
{
    public static CollectionStatus NoStatus = new CollectionStatus(1, "No Status");
    public static CollectionStatus EnumerationSynched = new CollectionStatus(2, "Enumeration Synched");
    
public CollectionStatus(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<CollectionStatus> List() =>
        new[] { NoStatus, EnumerationSynched };

    public static CollectionStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Collection Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static CollectionStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Collection Status: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}
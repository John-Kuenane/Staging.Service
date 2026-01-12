using Staging.Domain.SeedWork;

namespace Staging.Domain.AggregatesModel.FormAggregate;

public class DependencyRelationshipType
        : Enumeration
{
    public static DependencyRelationshipType And = new DependencyRelationshipType(1, "And");
    public static DependencyRelationshipType Or = new DependencyRelationshipType(2, "Or");
    public static DependencyRelationshipType NotSet = new DependencyRelationshipType(3, "Not Set");

    public DependencyRelationshipType(int id, string name)
        : base(id, name)
    {
    }

    public static IEnumerable<DependencyRelationshipType> List() =>
        new[] { And, Or, NotSet };

    public static DependencyRelationshipType FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new DomainException($"Possible values for Dependency Relation Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }

    public static DependencyRelationshipType From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new DomainException($"Possible values for Dependency Relation Type: {String.Join(",", List().Select(s => s.Name))}");
        }

        return state;
    }
}